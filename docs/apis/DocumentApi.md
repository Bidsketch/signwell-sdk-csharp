# SignWell.Sdk.Raw.DocumentApi

All URIs are relative to *https://www.signwell.com*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**CreateDocument**](DocumentApi.md#createdocument) | **POST** /api/v1/documents | Create Document |
| [**CreateDocumentFromTemplate**](DocumentApi.md#createdocumentfromtemplate) | **POST** /api/v1/document_templates/documents | Create Document from Template |
| [**DeleteDocument**](DocumentApi.md#deletedocument) | **DELETE** /api/v1/documents/{id} | Delete Document |
| [**GetCompletedPdf**](DocumentApi.md#getcompletedpdf) | **GET** /api/v1/documents/{id}/completed_pdf | Completed PDF |
| [**GetDocument**](DocumentApi.md#getdocument) | **GET** /api/v1/documents/{id} | Get Document |
| [**ListDocuments**](DocumentApi.md#listdocuments) | **GET** /api/v1/documents | List Documents |
| [**SendDocument**](DocumentApi.md#senddocument) | **POST** /api/v1/documents/{id}/send | Update and Send Document |
| [**SendReminder**](DocumentApi.md#sendreminder) | **POST** /api/v1/documents/{id}/remind | Send Reminder |
| [**UpdateRecipients**](DocumentApi.md#updaterecipients) | **PATCH** /api/v1/documents/{id}/recipients | Update Recipients |

<a id="createdocument"></a>
# **CreateDocument**
> DocumentResponse CreateDocument (DocumentRequest documentRequest)

Create Document

Creates and optionally sends a new document for signing. If `draft` is set to true the document will not be sent.


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **documentRequest** | [**DocumentRequest**](DocumentRequest.md) |  |  |

### Return type

[**DocumentResponse**](DocumentResponse.md)

### Authorization

[api_key](../README.md#api_key)

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **201** | created |  -  |
| **400** | bad request |  -  |
| **422** | unprocessable entity |  -  |
| **429** | rate limit exceeded |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="createdocumentfromtemplate"></a>
# **CreateDocumentFromTemplate**
> DocumentFromTemplateResponse CreateDocumentFromTemplate (DocumentFromTemplateRequest documentFromTemplateRequest)

Create Document from Template

Creates and optionally sends a new document for signing. If `draft` is set to true the document will not be sent.


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **documentFromTemplateRequest** | [**DocumentFromTemplateRequest**](DocumentFromTemplateRequest.md) |  |  |

### Return type

[**DocumentFromTemplateResponse**](DocumentFromTemplateResponse.md)

### Authorization

[api_key](../README.md#api_key)

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **201** | created |  -  |
| **400** | bad request |  -  |
| **422** | unprocessable entity |  -  |
| **429** | rate limit exceeded |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="deletedocument"></a>
# **DeleteDocument**
> void DeleteDocument (Guid id)

Delete Document

Deletes a document. Deleting a document will also cancel document signing (if in progress).  Supply the unique document ID from either a Create Document request or document page URL.


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **id** | **Guid** |  |  |

### Return type

void (empty response body)

### Authorization

[api_key](../README.md#api_key)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **204** | no content |  -  |
| **404** | not found |  -  |
| **429** | rate limit exceeded |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="getcompletedpdf"></a>
# **GetCompletedPdf**
> CompletedPdfResponse GetCompletedPdf (Guid id, bool urlOnly = null, bool auditPage = null, FileFormat fileFormat = null)

Completed PDF

Gets a completed document PDF or ZIP file. Supply the unique document ID from either a document creation request or document page URL.


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **id** | **Guid** |  |  |
| **urlOnly** | **bool** |  | [optional] [default to false] |
| **auditPage** | **bool** |  | [optional] [default to true] |
| **fileFormat** | **FileFormat** |  | [optional]  |

### Return type

[**CompletedPdfResponse**](CompletedPdfResponse.md)

### Authorization

[api_key](../README.md#api_key)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | successful, returns the final completed PDF, or if url_only is set to true, a JSON object is returned. When url_only&#x3D;false (default), the response is the raw PDF or ZIP binary data. |  -  |
| **404** | not_found |  -  |
| **429** | rate limit exceeded |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="getdocument"></a>
# **GetDocument**
> DocumentResponse GetDocument (Guid id)

Get Document

Returns a document and all associated document data. Supply the unique document ID from either a document creation request or Document page URL.


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **id** | **Guid** |  |  |

### Return type

[**DocumentResponse**](DocumentResponse.md)

### Authorization

[api_key](../README.md#api_key)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | successful |  -  |
| **404** | not_found |  -  |
| **429** | rate limit exceeded |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="listdocuments"></a>
# **ListDocuments**
> DocumentListResponse ListDocuments (int page = null, int limit = null, string query = null)

List Documents

Returns a paginated list of documents for the authenticated account.


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **page** | **int** |  | [optional] [default to 1] |
| **limit** | **int** |  | [optional] [default to 10] |
| **query** | **string** | Search documents using SignWell key:value syntax. | [optional]  |

### Return type

[**DocumentListResponse**](DocumentListResponse.md)

### Authorization

[api_key](../README.md#api_key)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | successful |  -  |
| **401** | unauthorized |  -  |
| **429** | rate limit exceeded |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="senddocument"></a>
# **SendDocument**
> DocumentResponse SendDocument (Guid id, UpdateDocumentAndSendRequest updateDocumentAndSendRequest)

Update and Send Document

Updates a draft document and sends it to be signed by recipients.


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **id** | **Guid** |  |  |
| **updateDocumentAndSendRequest** | [**UpdateDocumentAndSendRequest**](UpdateDocumentAndSendRequest.md) |  |  |

### Return type

[**DocumentResponse**](DocumentResponse.md)

### Authorization

[api_key](../README.md#api_key)

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **201** | created |  -  |
| **422** | unprocessable entity |  -  |
| **429** | rate limit exceeded |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="sendreminder"></a>
# **SendReminder**
> void SendReminder (Guid id, SendReminderRequest sendReminderRequest)

Send Reminder

Sends a reminder email to recipients that have not signed yet.


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **id** | **Guid** |  |  |
| **sendReminderRequest** | [**SendReminderRequest**](SendReminderRequest.md) |  |  |

### Return type

void (empty response body)

### Authorization

[api_key](../README.md#api_key)

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **201** | created |  -  |
| **422** | unprocessable entity |  -  |
| **404** | not found |  -  |
| **429** | rate limit exceeded |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="updaterecipients"></a>
# **UpdateRecipients**
> DocumentResponse UpdateRecipients (Guid id, UpdateRecipientsRequest updateRecipientsRequest)

Update Recipients

Updates one or more recipients on a document that has already been sent. Only recipients who have not started signing may be updated. Recipient IDs must be retrieved from the Get Document response. Allowed document statuses: sent, viewed, pending, bounced. For non-embedded documents, updated recipients will receive a new notification email. For embedded signing documents, email behavior follows each recipient's send_email setting.


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **id** | **Guid** |  |  |
| **updateRecipientsRequest** | [**UpdateRecipientsRequest**](UpdateRecipientsRequest.md) |  |  |

### Return type

[**DocumentResponse**](DocumentResponse.md)

### Authorization

[api_key](../README.md#api_key)

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | successful |  -  |
| **409** | conflict - document not in eligible state |  -  |
| **400** | bad request - invalid structure |  -  |
| **422** | unprocessable entity - business rule violation |  -  |
| **404** | not found |  -  |
| **429** | rate limit exceeded |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

