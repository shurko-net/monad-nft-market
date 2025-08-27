using Mcrio.Configuration.Provider.Docker.Secrets;
using MonadNftMarket.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MonadNftMarket.Context;
using MonadNftMarket.Filters;
using MonadNftMarket.Hubs;
using MonadNftMarket.Providers;
using MonadNftMarket.Services;
using MonadNftMarket.Services.EventParser;
using MonadNftMarket.Services.Monad;
using MonadNftMarket.Services.Notifications;
using MonadNftMarket.Services.Token;

var builder = WebApplication.CreateBuilder(args);
var myAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddDockerSecrets();

string cookieName =
    builder.Configuration.GetValue<string>(
        $"{nameof(EnvVariables)}:{nameof(EnvVariables.CookieName)}"
    )
    ?? throw new InvalidOperationException("CookieName is missing");

string jwtSecret =
    builder.Configuration.GetValue<string>(
        $"{nameof(EnvVariables)}:{nameof(EnvVariables.JwtTokenSecret)}"
    )
    ?? builder.Configuration.GetValue<string>(
        EnvVariables.ToDockerVariables(nameof(EnvVariables.JwtTokenSecret))
    )
    ?? throw new InvalidOperationException("JwtTokenSecret is missing");

builder.Services.Configure<EnvVariables>(options =>
{
    builder.Configuration
        .GetSection(nameof(EnvVariables))
        .Bind(options);

    foreach (var prop in typeof(EnvVariables).GetProperties())
    {
        var secretKey = EnvVariables.ToDockerVariables(prop.Name);
        var section = builder.Configuration.GetSection(secretKey);

        if (section.Exists() && !string.IsNullOrWhiteSpace(section.Value))
        {
            prop.SetValue(options, Convert.ChangeType(section.Value, prop.PropertyType));
        }
    }
});

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.Converters.Add(new BigIntegerJsonConverter());
        opts.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });
builder.Services.AddSignalR()
    .AddHubOptions<NotificationHub>(opts => opts.AddFilter<HubAuthorize>())
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.PropertyNameCaseInsensitive = true;
        options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IMagicEdenProvider, MagicEdenProvider>();
builder.Services.AddSingleton<IEventParser, EventParser>();
builder.Services.AddSingleton<IHyperSyncQuery, HyperSyncQuery>();
builder.Services.AddSingleton<IMonadService, MonadService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IUserIdentity, UserIdentity>();
builder.Services.AddSingleton<IUserIdProvider, WalletUserIdProvider>();
builder.Services.AddMemoryCache();

builder.Services.AddHostedService<RecordChanges>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: myAllowSpecificOrigins,
        policyBuilder =>
        {
            policyBuilder.WithOrigins("http://localhost:3000")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

builder.Services.AddDbContext<ApiDbContext>(options =>
{
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .UseSnakeCaseNamingConvention();
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    // options.RequireHttpsMetadata = true;
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;

    options.MapInboundClaims = false;

    options.TokenValidationParameters = new()
    {
        ValidateIssuerSigningKey = true,
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.ASCII.GetBytes(jwtSecret)),
        NameClaimType = JwtRegisteredClaimNames.Sub
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.TryGetValue(cookieName, out var token))
            {
                context.Token = token;
            }

            if (string.IsNullOrEmpty(context.Token))
            {
                context.Token = context.Request.Query[cookieName];
            }

            return Task.CompletedTask;
        }
    };
});
builder.Services.AddAuthorization();

var app = builder.Build();
app.UseHttpsRedirection();

app.UseCors(myAllowSpecificOrigins);

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.None,
    HttpOnly = HttpOnlyPolicy.Always,
    Secure = CookieSecurePolicy.Always
});
app.UseRouting();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");

app.Run();