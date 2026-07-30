# SignWell.Sdk.Models.DocumentTemplateResponse

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
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
**Placeholders** | [**List&lt;DocumentTemplateResponsePlaceholdersInner&gt;**](DocumentTemplateResponsePlaceholdersInner.md) |  | [optional] 
**CopiedPlaceholders** | [**List&lt;DocumentTemplateResponseCopiedPlaceholdersInner&gt;**](DocumentTemplateResponseCopiedPlaceholdersInner.md) |  | [optional] 
**Status** | **string** |  | [optional] 
**Reminders** | **bool** |  | [optional] 
**Archived** | **bool** |  | [optional] 
**EmbeddedEditUrl** | **string** |  | [optional] 
**TemplateLink** | **string** |  | [optional] 
**TemplateId** | **Guid** |  | [optional] 
**ApplySigningOrder** | **bool** |  | [optional] 
**RedirectUrl** | **string** |  | [optional] 
**DeclineRedirectUrl** | **string** |  | [optional] 
**Language** | **string** |  | [optional] 
**ExpiresIn** | **int** |  | [optional] 
**Files** | [**List&lt;FileInfo&gt;**](FileInfo.md) |  | [optional] 
**Fields** | **List&lt;List&lt;DocumentResponseFieldsInnerInner&gt;&gt;** |  | [optional] 
**AllowDecline** | **bool** |  | [optional] 
**AllowReassign** | **bool** |  | [optional] 
**Labels** | [**List&lt;LabelInfo&gt;**](LabelInfo.md) |  | [optional] 
**CheckboxGroups** | [**List&lt;CheckboxGroupInfo&gt;**](CheckboxGroupInfo.md) |  | [optional] 

[[Back to Model list]](../../README.md#documentation-for-models) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to README]](../../README.md)

