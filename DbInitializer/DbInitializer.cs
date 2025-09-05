using Microsoft.EntityFrameworkCore;
using MonadNftMarket.Context;

namespace MonadNftMarket.DbInitializer;

public class DbInitializer(IDbContextFactory<ApiDbContext> context) : IDbInitializer
{
    public void Initialize()
    {
        using var db = context.CreateDbContext();
        db.Database.Migrate();
    }
}