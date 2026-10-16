# SignWell.Sdk.Models.DocumentFromTemplateResponseFieldsInnerInner

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**X** | **float** |  | 
**Y** | **float** |  | 
**Page** | **int** |  | 
**Recipient** | [**DocumentFromTemplateResponseFieldsInnerInnerRecipient**](DocumentFromTemplateResponseFieldsInnerInnerRecipient.md) |  | [optional] 
**ApiId** | **string** |  | [optional] 
**Name** | **string** |  | [optional] 
**DateFormat** | **DateFormat** |  | [optional] 
**FixedWidth** | **bool** |  | [optional] 
**Formula** | **string** |  | [optional] 
**Label** | **string** |  | [optional] 
**LockSignDate** | **bool** |  | [optional] 
**Required** | **bool** |  | [optional] 
**Type** | **FieldType** |  | [optional] 
**Validation** | **TextValidation** |  | [optional] 
**ValidationRegex** | **string** | Text fields with custom validation only: the regex pattern used to validate field values. Required when validation is set to custom. | [optional] 
**ValidationRegexErrorMessage** | **string** | Text fields with custom validation only: the error message shown to the recipient when their input doesn&#39;t match the regex pattern. Required when validation is set to custom. | [optional] 
**Value** | [**DocumentResponseFieldsInnerInnerValue**](DocumentResponseFieldsInnerInnerValue.md) |  | [optional] 
**Height** | **string** |  | [optional] 
**Width** | **string** |  | [optional] 
**RecipientId** | **string** |  | [optional] 
**SigningElementsGroupId** | **Guid** |  | [optional] 
**PlaceholderName** | **string** |  | [optional] 
**Options** | [**List&lt;DocumentResponseFieldsInnerInnerOptionsInner&gt;**](DocumentResponseFieldsInnerInnerOptionsInner.md) | Dropdown options (for dropdown/select fields) | [optional] 
**DefaultOption** | **string** | Default selected option | [optional] 
**AllowOther** | **bool** | Whether \&quot;Other\&quot; option is allowed | [optional] 

[[Back to Model list]](../../README.md#documentation-for-models) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to README]](../../README.md)

