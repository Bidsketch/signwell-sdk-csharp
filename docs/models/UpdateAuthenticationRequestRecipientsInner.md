# SignWell.Sdk.Models.UpdateAuthenticationRequestRecipientsInner

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **string** | A unique identifier that you will give to each recipient. We recommend numbering sequentially from 1 to X. IDs are required for associating recipients to fields and more. | 
**Passcode** | **string** | Updated passcode for the recipient. If set, the signer will be required to enter the passcode before viewing and completing the document. On a 21 CFR Part 11 document SignWell generates the passcode, so a sender-defined passcode returns a 422. | [optional] 
**PasscodeDelivery** | [**PasscodeDelivery**](PasscodeDelivery.md) |  | [optional] 

[[Back to Model list]](../../README.md#documentation-for-models) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to README]](../../README.md)

