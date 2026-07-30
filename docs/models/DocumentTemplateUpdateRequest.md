# SignWell.Sdk.Models.DocumentTemplateUpdateRequest

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Name** | **string** | The name of the template. | [optional] 
**Subject** | **string** | Email subject for the signature request that recipients will see. Defaults to the default system subject or a template subject (if the document is created from a template). | [optional] 
**Message** | **string** | Email message for the signature request that recipients will see. Defaults to the default system message or a template message (if the document is created from a template). | [optional] 
**Draft** | **bool** | Whether the template can still be updated before it is ready for usage. If set to &#x60;false&#x60; the template is marked as &#x60;Available&#x60; and it will be ready for use. Defaults to &#x60;false&#x60;. | [optional] [default to false]
**ExpiresIn** | **int** | Number of days before the signature request expires. Defaults to the account expiration setting or template expiration (if the document is created from a template). | [optional] 
**Reminders** | **bool** | Whether to send signing reminders to recipients. Reminders are sent on day 3, day 6, and day 10 if set to &#x60;true&#x60;. Defaults to &#x60;true&#x60;. | [optional] [default to true]
**ApplySigningOrder** | **bool** | When set to &#x60;true&#x60; recipients will sign one at a time in the order of the &#x60;recipients&#x60; collection of this request. | [optional] [default to false]
**ApiApplicationId** | **Guid** | Unique identifier for API Application settings to use. API Applications are optional and mainly used when isolating OAuth apps or for more control over embedded API settings | [optional] 
**RedirectUrl** | **string** | A URL that recipients are redirected to after successfully signing a document. | [optional] 
**AllowDecline** | **bool** | Whether to allow recipients the option to decline signing a document. If multiple signers are involved in a document, any single recipient can cancel the entire document signing process by declining to sign. | [optional] [default to true]
**AllowReassign** | **bool** | In some cases a signer is not the right person to sign and may need to reassign their signing responsibilities to another person. This feature allows them to reassign the document to someone else. | [optional] [default to true]
**DeclineRedirectUrl** | **string** | A URL that recipients are redirected to if the document is declined. | [optional] 
**Metadata** | **Dictionary&lt;string, string&gt;** | Optional key-value data that can be associated with the document. If set, will be available every time the document data is returned. | [optional] 
**Labels** | [**List&lt;LabelRequest&gt;**](LabelRequest.md) | Labels can be used to organize documents in a way that can make it easy to find using the document search in SignWell. A document can have multiple labels. Updating labels on a document will replace any existing labels for that document. | [optional] 
**CheckboxGroups** | [**List&lt;TemplateCheckboxGroupsInner&gt;**](TemplateCheckboxGroupsInner.md) | Checkbox fields that are placed on a document can be grouped with selection requirements. At least 2 checkbox fields in an array of fields must be assigned to the same recipient. | [optional] 

[[Back to Model list]](../../README.md#documentation-for-models) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to README]](../../README.md)

