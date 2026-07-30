# SignWell.Sdk.Raw.ApiApplicationApi

All URIs are relative to *https://www.signwell.com*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**DeleteApiApplication**](ApiApplicationApi.md#deleteapiapplication) | **DELETE** /api/v1/api_applications/{id} | Delete API Application |
| [**GetApiApplication**](ApiApplicationApi.md#getapiapplication) | **GET** /api/v1/api_applications/{id} | Get API Application |

<a id="deleteapiapplication"></a>
# **DeleteApiApplication**
> void DeleteApiApplication (Guid id)

Delete API Application

Deletes an API Application from an account. Supply the unique Application ID from either the Create API Application response or the API Application edit page


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

<a id="getapiapplication"></a>
# **GetApiApplication**
> ApiApplicationResponse GetApiApplication (Guid id)

Get API Application

Gets the details of a specific API Application within an account. Supply the unique Application ID from either the Create API Application response or the API Application edit page.


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **id** | **Guid** |  |  |

### Return type

[**ApiApplicationResponse**](ApiApplicationResponse.md)

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

