# SignWell.Sdk.Raw.RegionalApi

All URIs are relative to *https://www.signwell.com*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**GetNom151Certificate**](RegionalApi.md#getnom151certificate) | **GET** /api/v1/documents/{id}/nom151_certificate | MX – NOM-151 Certificate |

<a id="getnom151certificate"></a>
# **GetNom151Certificate**
> Nom151UrlResponse GetNom151Certificate (Guid id, bool urlOnly = null, bool objectOnly = null)

MX – NOM-151 Certificate

Download NOM-151 certificate for a completed document. Returns a ZIP file, download URL, or raw certificate data based on query parameters.


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **id** | **Guid** |  |  |
| **urlOnly** | **bool** | If true, returns JSON with download URL instead of downloading the file | [optional] [default to false] |
| **objectOnly** | **bool** |  | [optional] [default to false] |

### Return type

[**Nom151UrlResponse**](Nom151UrlResponse.md)

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
| **422** | unprocessable_entity |  -  |
| **429** | rate limit exceeded |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

