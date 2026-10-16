# SignWell.Sdk.Models.RecipientsInnerPasscodeDelivery
Passcode delivery settings. When enabled, the system delivers the passcode to the signer via email. Always on for 21 CFR Part 11 documents and cannot be disabled.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Enabled** | **bool** | Enable passcode delivery for this recipient. Cannot be set to &#x60;false&#x60; on a 21 CFR Part 11 document: it returns a 422. | [optional] 
**Methods** | **List&lt;RecipientsInnerPasscodeDelivery.MethodsEnum&gt;** | Delivery methods for the passcode. Currently supports: email. Defaults to [\&quot;email\&quot;] when omitted. | [optional] 
**ExpireAfterAccess** | **bool** | When enabled, the passcode is cleared after successful authentication. If passcode_delivery is also enabled, a new passcode is auto-generated on the next send request. Otherwise the recipient will no longer need a passcode. | [optional] 

[[Back to Model list]](../../README.md#documentation-for-models) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to README]](../../README.md)

