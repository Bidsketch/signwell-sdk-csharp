# SignWell.Sdk.Raw.TemplateApi

All URIs are relative to *https://www.signwell.com*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**CreateTemplate**](TemplateApi.md#createtemplate) | **POST** /api/v1/document_templates | Create Template |
| [**DeleteTemplate**](TemplateApi.md#deletetemplate) | **DELETE** /api/v1/document_templates/{id} | Delete Template |
| [**GetTemplate**](TemplateApi.md#gettemplate) | **GET** /api/v1/document_templates/{id} | Get Template |
| [**ListTemplates**](TemplateApi.md#listtemplates) | **GET** /api/v1/document_templates | List Templates |
| [**UpdateTemplate**](TemplateApi.md#updatetemplate) | **PUT** /api/v1/document_templates/{id} | Update Template |

<a id="createtemplate"></a>
# **CreateTemplate**
> DocumentTemplateResponse CreateTemplate (DocumentTemplateRequest documentTemplateRequest)

Create Template

Creates a new template.


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **documentTemplateRequest** | [**DocumentTemplateRequest**](DocumentTemplateRequest.md) |  |  |

### Return type

[**DocumentTemplateResponse**](DocumentTemplateResponse.md)

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

<a id="deletetemplate"></a>
# **DeleteTemplate**
> void DeleteTemplate (Guid id)

Delete Template

Deletes a template. Supply the unique template ID from either a Create Template request or template page URL.


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

<a id="gettemplate"></a>
# **GetTemplate**
> DocumentTemplateResponse GetTemplate (Guid id)

Get Template

Returns a template and all associated template data. Supply the unique template ID from either a Create Template request or template page URL.


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **id** | **Guid** |  |  |

### Return type

[**DocumentTemplateResponse**](DocumentTemplateResponse.md)

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

<a id="listtemplates"></a>
# **ListTemplates**
> DocumentTemplateListResponse ListTemplates (int page = null, int limit = null, string query = null)

List Templates

Returns a paginated list of templates for the authenticated account.


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **page** | **int** |  | [optional] [default to 1] |
| **limit** | **int** |  | [optional] [default to 10] |
| **query** | **string** | Search templates using SignWell key:value syntax. | [optional]  |

### Return type

[**DocumentTemplateListResponse**](DocumentTemplateListResponse.md)

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

<a id="updatetemplate"></a>
# **UpdateTemplate**
> DocumentTemplateResponse UpdateTemplate (Guid id, DocumentTemplateUpdateRequest documentTemplateUpdateRequest)

Update Template

Updates an existing template.


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **id** | **Guid** |  |  |
| **documentTemplateUpdateRequest** | [**DocumentTemplateUpdateRequest**](DocumentTemplateUpdateRequest.md) |  |  |

### Return type

[**DocumentTemplateResponse**](DocumentTemplateResponse.md)

### Authorization

[api_key](../README.md#api_key)

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | ok |  -  |
| **400** | bad request |  -  |
| **422** | unprocessable entity |  -  |
| **429** | rate limit exceeded |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

