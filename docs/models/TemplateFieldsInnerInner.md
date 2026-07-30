# SignWell.Sdk.Models.TemplateFieldsInnerInner

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**X** | **float** | Horizontal value in the coordinates of the field (in pixels). Coordinates are specific to the page where fields are located. | 
**Y** | **float** | Vertical value in the coordinates of the field (in pixels). Coordinates are specific to the page where fields are located. | 
**Page** | **int** | The page number within the file. If the page does not exist within the file then the field won&#39;t be created. | 
**PlaceholderId** | **string** | Unique identifier of the placeholder assigned to the field. | 
**Type** | **FieldType** |  | 
**Required** | **bool** | Whether the field must be completed by the recipient. Defaults to &#x60;true&#x60; except for checkbox type fields. | [optional] [default to true]
**Label** | **string** | Text and Date fields only: label that is displayed when the field is empty. | [optional] 
**Value** | [**AdditionalFieldsInnerInnerValue**](AdditionalFieldsInnerInnerValue.md) |  | [optional] 
**ApiId** | **string** | Unique identifier of the field. Useful when needing to reference specific field values or update a document and its fields. | [optional] 
**Name** | **string** | Checkbox fields only. At least 2 checkbox fields in an array of fields must be assigned to the same recipient and grouped with selection requirements. | [optional] 
**Validation** | **TextValidation** |  | [optional] 
**FixedWidth** | **bool** | Text fields only: whether the field width will stay fixed and text will display in multiple lines, rather than one long line. If set to &#x60;false&#x60; the field width will automatically grow horizontally to fit text on one line. Defaults to &#x60;false&#x60;. | [optional] [default to false]
**LockSignDate** | **bool** | Date fields only: makes fields readonly and automatically populates with the date the recipient signed. Defaults to &#x60;false&#x60;. | [optional] [default to false]
**DateFormat** | **DateFormat** |  | [optional] 
**Height** | **float** | Height of the field (in pixels). Maximum height varies by field type: Signature/Initials (200px), others (74px). When using text tags if the height is greater than the maximum height, the height will be set to the maximum height. | [optional] 
**Width** | **float** | Width of the field (in pixels). For text fields, width will auto-grow unless &#x60;fixed_width&#x60; is true. | [optional] 
**Options** | [**List&lt;DropdownOption&gt;**](DropdownOption.md) | Array of dropdown options (for dropdown/select fields only) | [optional] 
**DefaultOption** | **string** | Default selected option (for dropdown/select fields only) | [optional] 
**AllowOther** | **bool** | Whether to allow \&quot;Other\&quot; option with text input (for dropdown/select fields only) | [optional] [default to false]

[[Back to Model list]](../../README.md#documentation-for-models) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to README]](../../README.md)

