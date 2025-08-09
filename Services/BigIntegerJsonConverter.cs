using System.Globalization;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MonadNftMarket.Services;

public class BigIntegerJsonConverter : JsonConverter<BigInteger>
{
    public override BigInteger Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var s = reader.GetString()!;
            return BigInteger.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            var raw = Encoding.UTF8.GetString(reader.ValueSpan);
            return BigInteger.Parse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }
        
        throw new JsonException($"Unexpected token parsing BigInteger: {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, BigInteger value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
    }
}