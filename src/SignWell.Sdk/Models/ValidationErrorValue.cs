using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SignWell.Sdk.Models;

[JsonConverter(typeof(ValidationErrorValueJsonConverter))]
public sealed class ValidationErrorValue : IValidatableObject
{
    public ValidationErrorValue(string value) => String = value;
    public ValidationErrorValue(IReadOnlyList<string> value) => List = value;
    public ValidationErrorValue(IReadOnlyDictionary<string, ValidationErrorValue> value) => Dictionary = value;

    public string? String { get; }
    public IReadOnlyList<string>? List { get; }
    public IReadOnlyDictionary<string, ValidationErrorValue>? Dictionary { get; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        yield break;
    }
}

public sealed class ValidationErrorValueJsonConverter : JsonConverter<ValidationErrorValue>
{
    public override ValidationErrorValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
            return new ValidationErrorValue(reader.GetString() ?? string.Empty);
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var values = JsonSerializer.Deserialize<List<string>>(ref reader, options)
                ?? throw new JsonException("Validation error array cannot be null.");
            return new ValidationErrorValue(values);
        }
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            var values = new Dictionary<string, ValidationErrorValue>(StringComparer.Ordinal);
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException();
                var name = reader.GetString() ?? throw new JsonException();
                if (!reader.Read()) throw new JsonException();
                values[name] = Read(ref reader, typeof(ValidationErrorValue), options);
            }
            return new ValidationErrorValue(values);
        }
        throw new JsonException("Validation error values must be strings, string arrays, or nested objects.");
    }

    public override void Write(Utf8JsonWriter writer, ValidationErrorValue value, JsonSerializerOptions options)
    {
        if (value.String is not null)
        {
            writer.WriteStringValue(value.String);
            return;
        }
        if (value.List is not null)
        {
            JsonSerializer.Serialize(writer, value.List, options);
            return;
        }
        if (value.Dictionary is not null)
        {
            writer.WriteStartObject();
            foreach (var pair in value.Dictionary)
            {
                writer.WritePropertyName(pair.Key);
                Write(writer, pair.Value, options);
            }
            writer.WriteEndObject();
            return;
        }
        writer.WriteNullValue();
    }
}
