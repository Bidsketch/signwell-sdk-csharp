# SignWell.Sdk.Models.DocumentFromTemplateResponseRecipientsInner

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **string** |  | [optional] 
**Name** | **string** |  | [optional] 
**Email** | **string** |  | [optional] 
**Role** | **string** |  | [optional] 
**Message** | **string** |  | [optional] 
**Subject** | **string** |  | [optional] 
**SendEmail** | **bool** |  | [optional] 
**SendEmailDelay** | **int** |  | [optional] 
**DeliveryMethod** | **string** | How the recipient is notified: email (default), sms, or email_and_sms. | [optional] 
**Sms** | [**DocumentResponseRecipientsInnerSms**](DocumentResponseRecipientsInnerSms.md) |  | [optional] 
**SigningOrder** | **int** |  | [optional] 
**SigningUrl** | **string** |  | [optional] 
**EmbeddedSigningUrl** | **string** |  | [optional] 
**Bounced** | **bool** |  | [optional] 
**BouncedDetails** | **string** |  | [optional] 
**AttachmentRequests** | [**List&lt;AttachmentRequestInfo&gt;**](AttachmentRequestInfo.md) |  | [optional] 
**Passcode** | **string** |  | [optional] 
**PasscodeDelivery** | [**DocumentResponseRecipientsInnerPasscodeDelivery**](DocumentResponseRecipientsInnerPasscodeDelivery.md) |  | [optional] 
**Status** | **string** |  | [optional] 
**PlaceholderName** | **string** |  | [optional] 

[[Back to Model list]](../../README.md#documentation-for-models) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to README]](../../README.md)

