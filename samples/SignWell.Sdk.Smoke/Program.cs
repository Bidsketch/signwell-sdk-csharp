using System;
using SignWell.Sdk;

using var client = new SignWellClient("package-asset-selection-smoke-test");
Console.WriteLine(client.Documents.Raw.GetType().FullName);
