# SignWell.Sdk.Models.DocumentResponse

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**TestMode** | **bool** |  | 
**Id** | **Guid** |  | 
**ApiApplicationId** | **Guid** |  | [optional] 
**RequesterEmailAddress** | **string** |  | [optional] 
**CustomRequesterName** | **string** |  | [optional] 
**CustomRequesterEmail** | **string** |  | [optional] 
**Name** | **string** |  | [optional] 
**Subject** | **string** |  | [optional] 
**Message** | **string** |  | [optional] 
**Metadata** | **Dictionary&lt;string, string&gt;** |  | [optional] 
**CreatedAt** | **DateTimeOffset** |  | [optional] 
**UpdatedAt** | **DateTimeOffset** |  | [optional] 
**Recipients** | [**List&lt;DocumentResponseRecipientsInner&gt;**](DocumentResponseRecipientsInner.md) |  | [optional] 
**Status** | **string** | Possible values: Draft, Created, Sending, Sent, Pending, Viewed, Completed, Manually completed, Declined, Canceled, Bounced, Blocked, Error, Expired | [optional] 
**Reminders** | **bool** |  | [optional] 
**Archived** | **bool** |  | [optional] 
**EmbeddedSigning** | **bool** |  | [optional] 
**EmbeddedEditUrl** | **string** |  | [optional] 
**EmbeddedPreviewUrl** | **string** |  | [optional] 
**ApplySigningOrder** | **bool** |  | [optional] 
**RedirectUrl** | **string** |  | [optional] 
**DeclineRedirectUrl** | **string** |  | [optional] 
**Language** | **string** |  | [optional] 
**ExpiresIn** | **int** |  | [optional] 
**DeclineMessage** | **string** |  | [optional] 
**ErrorMessage** | **string** |  | [optional] 
**TemplateId** | **Guid** |  | [optional] 
**TemplateIds** | **List&lt;Guid&gt;** |  | [optional] 
**EmbeddedSigningNotifications** | **bool** |  | [optional] 
**AttachmentRequests** | [**List&lt;DocumentResponseAttachmentRequestsInner&gt;**](DocumentResponseAttachmentRequestsInner.md) |  | [optional] 
**Files** | [**List&lt;FileInfo&gt;**](FileInfo.md) |  | [optional] 
**CopiedContacts** | [**List&lt;CopiedContactInfo&gt;**](CopiedContactInfo.md) |  | [optional] 
**Fields** | **List&lt;List&lt;DocumentResponseFieldsInnerInner&gt;&gt;** |  | [optional] 
**AllowDecline** | **bool** |  | [optional] 
**AllowReassign** | **bool** |  | [optional] 
**Labels** | [**List&lt;LabelInfo&gt;**](LabelInfo.md) |  | [optional] 
**CheckboxGroups** | [**List&lt;CheckboxGroupInfo&gt;**](CheckboxGroupInfo.md) |  | [optional] 

[[Back to Model list]](../../README.md#documentation-for-models) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to README]](../../README.md)

