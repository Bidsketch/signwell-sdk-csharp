# SignWell.Sdk.Raw.BulkSendApi

All URIs are relative to *https://www.signwell.com*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**CreateBulkSend**](BulkSendApi.md#createbulksend) | **POST** /api/v1/bulk_sends | Create Bulk Send |
| [**GetBulkSend**](BulkSendApi.md#getbulksend) | **GET** /api/v1/bulk_sends/{id} | Get Bulk Send |
| [**GetBulkSendCsvTemplate**](BulkSendApi.md#getbulksendcsvtemplate) | **GET** /api/v1/bulk_sends/csv_template | Get Bulk Send CSV Template |
| [**GetBulkSendDocuments**](BulkSendApi.md#getbulksenddocuments) | **GET** /api/v1/bulk_sends/{id}/documents | Get Bulk Send Documents |
| [**ListBulkSends**](BulkSendApi.md#listbulksends) | **GET** /api/v1/bulk_sends | List Bulk Sendings |
| [**ValidateBulkSendCsv**](BulkSendApi.md#validatebulksendcsv) | **POST** /api/v1/bulk_sends/validate_csv | Validate Bulk Send CSV |

<a id="createbulksend"></a>
# **CreateBulkSend**
> BulkSendCreateResponse CreateBulkSend (CreateBulkSendRequest createBulkSendRequest)

Create Bulk Send

Creates a bulk send, and it validates the CSV file before creating the bulk send.


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **createBulkSendRequest** | [**CreateBulkSendRequest**](CreateBulkSendRequest.md) |  |  |

### Return type

[**BulkSendCreateResponse**](BulkSendCreateResponse.md)

### Authorization

[api_key](../README.md#api_key)

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **201** | successful |  -  |
| **422** | unprocessable entity |  -  |
| **401** | unauthorized |  -  |
| **429** | rate limit exceeded |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="getbulksend"></a>
# **GetBulkSend**
> BulkSendResponse GetBulkSend (Guid id)

Get Bulk Send

Returns information about the Bulk Send.


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **id** | **Guid** |  |  |

### Return type

[**BulkSendResponse**](BulkSendResponse.md)

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
| **404** | not found |  -  |
| **429** | rate limit exceeded |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="getbulksendcsvtemplate"></a>
# **GetBulkSendCsvTemplate**
> System.IO.Stream GetBulkSendCsvTemplate (List<Guid> templateIds, bool base64 = null)

Get Bulk Send CSV Template

Fetches a CSV template that corresponds to the provided document template IDs. CSV templates are blank CSV files that have columns containing required and optional data that can be sent when creating a bulk send. Fields can be referenced by the field label. Example: [placeholder name]_[field label] could be something like customer_address or signer_company_name (if 'Customer' and 'Signer' were placeholder names for templates set up in SignWell).


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **templateIds** | [**List&lt;Guid&gt;**](Guid.md) |  |  |
| **base64** | **bool** |  | [optional]  |

### Return type

**System.IO.Stream**

### Authorization

[api_key](../README.md#api_key)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/octet-stream, application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | successful |  -  |
| **401** | unauthorized |  -  |
| **404** | not found |  -  |
| **429** | rate limit exceeded |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="getbulksenddocuments"></a>
# **GetBulkSendDocuments**
> BulkSendDocumentsResponse GetBulkSendDocuments (Guid id, int limit = null, int page = null)

Get Bulk Send Documents

Returns information about the Bulk Send.


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **id** | **Guid** |  |  |
| **limit** | **int** |  | [optional] [default to 10] |
| **page** | **int** |  | [optional] [default to 1] |

### Return type

[**BulkSendDocumentsResponse**](BulkSendDocumentsResponse.md)

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
| **404** | not found |  -  |
| **429** | rate limit exceeded |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="listbulksends"></a>
# **ListBulkSends**
> BulkSendListResponse ListBulkSends (string userEmail = null, int limit = null, int page = null, Guid apiApplicationId = null)

List Bulk Sendings

Returns information about the Bulk Send.


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **userEmail** | **string** |  | [optional]  |
| **limit** | **int** |  | [optional] [default to 10] |
| **page** | **int** |  | [optional] [default to 1] |
| **apiApplicationId** | **Guid** |  | [optional]  |

### Return type

[**BulkSendListResponse**](BulkSendListResponse.md)

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

<a id="validatebulksendcsv"></a>
# **ValidateBulkSendCsv**
> BulkSendValidateCsvResponse ValidateBulkSendCsv (BulkSendCsvRequest bulkSendCsvRequest)

Validate Bulk Send CSV

Validates a Bulk Send CSV file before creating the Bulk Send. It will check the structure of the CSV and the data it contains, and return any errors found.


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **bulkSendCsvRequest** | [**BulkSendCsvRequest**](BulkSendCsvRequest.md) |  |  |

### Return type

[**BulkSendValidateCsvResponse**](BulkSendValidateCsvResponse.md)

### Authorization

[api_key](../README.md#api_key)

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | successful |  -  |
| **422** | unprocessable entity |  -  |
| **401** | unauthorized |  -  |
| **429** | rate limit exceeded |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

