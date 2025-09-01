using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MonadNftMarket.Services;

public static class CursorService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    
    private static string Base64UrlEncode(byte[] input) =>
        Convert.ToBase64String(input).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] Base64UrlDecode(string input)
    {
        var s = input.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4)
        {
            case 2: s += "=="; break;
            case 3: s += "="; break;
            case 0: break;
            default: throw new Exception("Invalid base64url string!");
        }
        
        return Convert.FromBase64String(s);
    }

    private static byte[] ComputeHmac(byte[] payload, byte[] key)
    {
        using var hmac = new HMACSHA256(key);
        return hmac.ComputeHash(payload);
    }

    public static string Encode<T>(T cursor, byte[]? hmacKey = null)
    {
        if(cursor is null) throw new ArgumentNullException(nameof(cursor));
        
        var json = JsonSerializer.Serialize(cursor, JsonOptions);
        var payload = Encoding.UTF8.GetBytes(json);
        var encodedPayload = Base64UrlEncode(payload);
        
        if(hmacKey == null) return encodedPayload;
        
        var sigBytes = ComputeHmac(payload, hmacKey);
        var encodeSig = Base64UrlEncode(sigBytes);

        return $"{encodedPayload}.{encodeSig}";
    }

    public static T? Decode<T>(string token, byte[]? hmacKey = null)
    {
        if(string.IsNullOrEmpty(token)) throw new ArgumentNullException(nameof(token));

        if (hmacKey == null)
        {
            var payload = Base64UrlDecode(token);
            var jsonPayload = Encoding.UTF8.GetString(payload);
            return JsonSerializer.Deserialize<T>(jsonPayload, JsonOptions);
        }

        var parts = token.Split('.', 2);
        if(parts.Length != 2) throw new FormatException("Invalid token format (expected payload.signature).");
        
        var payloadBytes = Base64UrlDecode(parts[0]);
        var sigBytes = Base64UrlDecode(parts[1]);

        var expectedSig = ComputeHmac(payloadBytes, hmacKey);
        
        if(!CryptographicOperations.FixedTimeEquals(sigBytes, expectedSig))
            throw new CryptographicException("Cursor signature validation failed.");
        
        var json = Encoding.UTF8.GetString(payloadBytes);
        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }
}