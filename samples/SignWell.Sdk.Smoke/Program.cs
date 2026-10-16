using System;
using SignWell.Sdk;
using SignWell.Sdk.Models;

var recipient = new RecipientsInner("1", "jane@example.com", "Jane", "code", "Subject", "Message", true, 5);
var templateRecipient = new TemplateRecipientsInner("1", "jane@example.com", "Jane", "Signer", "code", "Subject", "Message", true, 5);
if (recipient.Email != "jane@example.com" || recipient.Name != "Jane" || templateRecipient.PlaceholderName != "Signer")
    throw new InvalidOperationException("Legacy recipient constructors changed their argument meanings.");

using var client = new SignWellClient("package-asset-selection-smoke-test");
Console.WriteLine(client.Documents.Raw.GetType().FullName);
