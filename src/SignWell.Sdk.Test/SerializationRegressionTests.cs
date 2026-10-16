using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using SignWell.Sdk.Client;
using SignWell.Sdk.Models;
using Xunit;

namespace SignWell.Sdk.Test;

public sealed class SerializationRegressionTests
{
    private static JsonSerializerOptions Options()
    {
        JsonSerializerOptions options = null!;
        new HostConfiguration(new ServiceCollection()).ConfigureJsonOptions(value => options = value);
        return options;
    }

    [Theory]
    [InlineData("")]
    [InlineData(",\"email\":null")]
    [InlineData(",\"email\":\"jane@example.com\"")]
    public void SmsRecipientResponsesAcceptMissingNullAndPresentEmail(string emailProperty)
    {
        var options = Options();
        var json = "{\"name\":\"Jane\",\"delivery_method\":\"sms\"" + emailProperty + "}";
        Type[] types = [typeof(DocumentResponseRecipientsInner), typeof(DocumentFromTemplateResponseRecipientsInner)];
        foreach (var type in types)
        {
            var recipient = JsonSerializer.Deserialize(json, type, options)!;
            using var body = JsonDocument.Parse(JsonSerializer.Serialize(recipient, type, options));
            Assert.Equal("sms", body.RootElement.GetProperty("delivery_method").GetString());
            if (emailProperty.Length == 0)
                Assert.False(body.RootElement.TryGetProperty("email", out _));
            else
                Assert.Equal(emailProperty.Contains("jane@example.com") ? "jane@example.com" : null,
                    body.RootElement.GetProperty("email").GetString());
        }
    }

    [Fact]
    public void LegacyResponseRecipientConstructorsKeepEmailBeforeId()
    {
        var recipient = new DocumentResponseRecipientsInner("Jane", "jane@example.com", "recipient-id");
        Assert.Equal("jane@example.com", recipient.Email);
        Assert.Equal("recipient-id", recipient.Id);
        var template = new DocumentFromTemplateResponseRecipientsInner("jane@example.com", "recipient-id", "Jane");
        Assert.Equal("jane@example.com", template.Email);
        Assert.Equal("recipient-id", template.Id);
        Assert.Equal("Jane", template.Name);
    }

    [Fact]
    public void DropdownFieldsRoundTripStringAndDetailedOptions()
    {
        var options = Options();
        var field = new FieldsInnerInner(1, 2, 1, "1", FieldType.Dropdown)
        {
            Options = [new DropdownOption("A"), new DropdownOption("B"), new DropdownOption(new DetailedOption("C", apiId: "c"))]
        };
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(field, options));
        var choices = json.RootElement.GetProperty("options");
        Assert.Equal("A", choices[0].GetString());
        Assert.Equal("B", choices[1].GetString());
        Assert.Equal("C", choices[2].GetProperty("name").GetString());
        Assert.Equal("c", choices[2].GetProperty("api_id").GetString());
        var read = JsonSerializer.Deserialize<FieldsInnerInner>(json.RootElement.GetRawText(), options)!;
        Assert.Equal("A", read.Options![0].String);
        Assert.Equal("C", read.Options[2].DetailedOption!.Name);
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<DropdownOption>("42", options));
    }

    [Fact]
    public void EveryPasscodeMethodsArrayUsesApiWireValues()
    {
        var options = Options();
        object[] deliveries =
        [
            new RecipientsInnerPasscodeDelivery { Methods = [RecipientsInnerPasscodeDelivery.MethodsEnum.Email] },
            new PlaceholdersInnerPasscodeDelivery { Methods = [PlaceholdersInnerPasscodeDelivery.MethodsEnum.Email] },
            new PasscodeDelivery { Methods = [PasscodeDelivery.MethodsEnum.Email] },
            new DocumentResponseRecipientsInnerPasscodeDelivery { Methods = ["email"] }
        ];
        foreach (var delivery in deliveries)
        {
            var json = JsonSerializer.Serialize(delivery, delivery.GetType(), options);
            using var body = JsonDocument.Parse(json);
            Assert.Equal("email", body.RootElement.GetProperty("methods")[0].GetString());
            Assert.Equal(json, JsonSerializer.Serialize(JsonSerializer.Deserialize(json, delivery.GetType(), options), delivery.GetType(), options));
        }
    }

    [Fact]
    public void AllModelEnumsAcceptFutureValuesAndSerializeKnownWireValues()
    {
        var options = Options();
        foreach (var type in typeof(DateFormat).Assembly.GetTypes().Where(type => type.IsEnum && type.Namespace == "SignWell.Sdk.Models"))
        {
            var unknown = Enum.Parse(type, "UnknownDefaultOpenApi");
            Assert.Equal(unknown, JsonSerializer.Deserialize("\"future_value\"", type, options));
            var container = type.DeclaringType ?? type.Assembly.GetType(type.FullName + "ValueConverter")!;
            var helper = container.GetMethod(type.DeclaringType is null ? "FromString" : type.Name + "FromString")!;
            Assert.Equal(unknown, helper.Invoke(null, ["future_value"]));
            foreach (var value in Enum.GetValues(type))
            {
                var json = JsonSerializer.Serialize(value, type, options);
                Assert.Equal(value, JsonSerializer.Deserialize(json, type, options));
            }
        }
        Assert.Equal("\"MM/DD/YYYY\"", JsonSerializer.Serialize(DateFormat.MMDDYYYY, options));
        Assert.Equal(DateFormat.UnknownDefaultOpenApi, DateFormatValueConverter.FromString("future_format"));
        var recipient = JsonSerializer.Deserialize<RecipientsInner>("""{"id":"1","delivery_method":"future_method"}""", options)!;
        Assert.Equal(RecipientsInner.DeliveryMethodEnum.UnknownDefaultOpenApi, recipient.DeliveryMethod);
        var field = JsonSerializer.Deserialize<DocumentResponseFieldsInnerInner>("""{"x":1,"y":2,"page":1,"date_format":"future_format","type":"future_type"}""", options)!;
        Assert.Equal(DateFormat.UnknownDefaultOpenApi, field.DateFormat);
        Assert.Equal(FieldType.UnknownDefaultOpenApi, field.Type);
    }

    [Fact]
    public void EveryPrimitiveUnionModelPreservesScalarValues()
    {
        var options = Options();
        Type[] types = [typeof(AdditionalFieldsInnerInnerValue), typeof(DocumentResponseFieldsInnerInnerValue), typeof(TemplateFieldValuesInnerValue)];
        foreach (var type in types)
        {
            foreach (var json in new[] { "\"t\"", "\"f\"", "\"123\"", "\"\"", "false", "true", "0", "12.5", "null" })
            {
                var value = JsonSerializer.Deserialize(json, type, options);
                Assert.Equal(json, JsonSerializer.Serialize(value, type, options));
            }
        }
    }

    [Fact]
    public void LegacyRecipientConstructorsKeepEveryPositionalArgument()
    {
        var recipient = new RecipientsInner("1", "jane@example.com", "Jane", "code", "Subject", "Message", true, 5);
        Assert.Equal("jane@example.com", recipient.Email);
        Assert.Equal("Jane", recipient.Name);
        Assert.Equal("code", recipient.Passcode);
        Assert.Equal("Subject", recipient.Subject);
        Assert.Equal("Message", recipient.Message);
        Assert.True(recipient.SendEmail);
        Assert.Equal(5, recipient.SendEmailDelay);
        var template = new TemplateRecipientsInner("1", "jane@example.com", "Jane", "Signer", "code", "Subject", "Message", true, 5);
        Assert.Equal("jane@example.com", template.Email);
        Assert.Equal("Jane", template.Name);
        Assert.Equal("Signer", template.PlaceholderName);
        Assert.Equal("Subject", template.Subject);
        Assert.Equal(5, template.SendEmailDelay);
        var sms = new RecipientsInner("1", name: "Jane", deliveryMethod: RecipientsInner.DeliveryMethodEnum.Sms, phoneNumber: "+14155551234");
        Assert.Equal("+14155551234", sms.PhoneNumber);
        Assert.Null(sms.Email);
    }
}
