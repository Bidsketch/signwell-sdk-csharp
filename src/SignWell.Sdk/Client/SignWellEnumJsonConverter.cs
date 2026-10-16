using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SignWell.Sdk.Client;

internal sealed class SignWellEnumJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsEnum && typeToConvert.Namespace == "SignWell.Sdk.Models";

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
        (JsonConverter)Activator.CreateInstance(typeof(SignWellEnumJsonConverter<>).MakeGenericType(typeToConvert))!;
}

internal sealed class SignWellEnumJsonConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum
{
    private readonly Dictionary<TEnum, string> _wireValues = new();
    private readonly Dictionary<string, TEnum> _enumValues = new(StringComparer.Ordinal);
    private readonly TEnum _unknown;

    public SignWellEnumJsonConverter()
    {
        var type = typeof(TEnum);
        var container = type.DeclaringType ?? type.Assembly.GetType(type.FullName + "ValueConverter")!;
        var method = container.GetMethod(type.DeclaringType is null ? "ToJsonValue" : type.Name + "ToJsonValue", BindingFlags.Public | BindingFlags.Static)!;
        foreach (TEnum value in Enum.GetValues(type))
        {
            var wireValue = (string)method.Invoke(null, new object[] { value })!;
            _wireValues.Add(value, wireValue);
            _enumValues.Add(wireValue, value);
        }
        _unknown = (TEnum)Enum.Parse(type, "UnknownDefaultOpenApi");
    }

    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String) throw new JsonException("Enum values must be strings.");
        return _enumValues.TryGetValue(reader.GetString()!, out var value) ? value : _unknown;
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        if (!_wireValues.TryGetValue(value, out var wireValue)) throw new JsonException("Invalid enum value.");
        writer.WriteStringValue(wireValue);
    }
}

public partial class HostConfiguration
{
    partial void OnHostConfigurationCreated() =>
        _jsonOptions.Converters.Insert(0, new SignWellEnumJsonConverterFactory());

    partial void OnAddApiHttpClientBuilder(IHttpClientBuilder builder, Action<IHttpClientBuilder>? userBuilder, ref bool suppressDefault) =>
        builder.AddHttpMessageHandler(provider => new RateLimitRetryHandler(
            provider.GetService<SignWellClientOptions>()?.MaxRateLimitRetries ?? 2));
}
