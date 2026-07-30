# SignWell.Sdk.Models.TemplateCheckboxGroupsInner

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**GroupName** | **string** | A unique identifier for the checkbox group. | 
**PlaceholderId** | **string** | The recipient ID associated with the checkbox group. | 
**CheckboxIds** | **List&lt;string&gt;** |  | 
**Validation** | **CheckboxValidation** |  | [optional] 
**Required** | **bool** | Whether the group must be completed by the recipient. Defaults to false. | [optional] [default to false]
**MinValue** | **int** | The minimum number of checkboxes that must be checked in the group. (Only for validation: minimum and range) | [optional] 
**MaxValue** | **int** | The maximum number of checkboxes that can be checked in the group. (Only for validation: maximum and range) | [optional] 
**ExactValue** | **int** | The exact number of checkboxes that must be checked in the group. (Only for validation: exact) | [optional] 

[[Back to Model list]](../../README.md#documentation-for-models) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to README]](../../README.md)

