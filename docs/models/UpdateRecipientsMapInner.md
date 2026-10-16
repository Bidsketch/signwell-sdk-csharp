# SignWell.Sdk.Models.UpdateRecipientsMapInner

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **string** | The recipient&#39;s unique identifier from the Get Document response. | 
**Name** | **string** | Updated name for the recipient. | 
**Email** | **string** | Updated email address for the recipient. | 
**Subject** | **string** | Updated email subject for the signature request that this recipient will see. | [optional] 
**Message** | **string** | Updated email message for the signature request that this recipient will see. | [optional] 
**Passcode** | **string** | Updated passcode for the recipient. If set, the signer will be required to enter the passcode before viewing and completing the document. On a 21 CFR Part 11 document SignWell generates the passcode, so a sender-defined passcode returns a 422. | [optional] 
**PasscodeDelivery** | [**PasscodeDelivery**](PasscodeDelivery.md) |  | [optional] 

[[Back to Model list]](../../README.md#documentation-for-models) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to README]](../../README.md)

