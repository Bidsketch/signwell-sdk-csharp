# SignWell.Sdk.Models.PlaceholdersInnerPasscodeDelivery
Passcode delivery (OTP) settings for this placeholder. When enabled, documents created from this template inherit these settings for the corresponding recipient. While 21 CFR Part 11 is enabled for the workspace, a placeholder cannot disable passcode delivery: it returns a 422.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Enabled** | **bool** | Enable passcode delivery for this recipient. Cannot be set to &#x60;false&#x60; on a 21 CFR Part 11 document: it returns a 422. | [optional] 
**Methods** | **List&lt;PlaceholdersInnerPasscodeDelivery.MethodsEnum&gt;** | Delivery methods for the passcode. Currently supports: email. Defaults to [\&quot;email\&quot;] when omitted. | [optional] 
**ExpireAfterAccess** | **bool** | Whether the passcode expires after the recipient accesses the document. Inherited by documents created from this template. | [optional] 

[[Back to Model list]](../../README.md#documentation-for-models) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to README]](../../README.md)

