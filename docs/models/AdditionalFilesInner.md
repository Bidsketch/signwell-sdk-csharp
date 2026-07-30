# SignWell.Sdk.Models.AdditionalFilesInner
Additional files to be appended to the document. Will not replace existing files from the template. Document files can be uploaded by specifying a file URL or base64 string. Either `file_url` or `file_base64` must be present (not both). Valid file types are: .pdf, .docx, .jpg, .png, .ppt, .xls, .pages, and .txt.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Name** | **string** | Name of the file that will be uploaded. | 
**FileUrl** | **string** | Publicly available URL of the file to be uploaded. | [optional] 
**FileBase64** | **byte[]** | A RFC 4648 base64 string of the file to be uploaded. | [optional] 

[[Back to Model list]](../../README.md#documentation-for-models) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to README]](../../README.md)

