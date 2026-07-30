# SignWell.Sdk.Raw.MeApi

All URIs are relative to *https://www.signwell.com*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**GetMe**](MeApi.md#getme) | **GET** /api/v1/me | Get credentials |

<a id="getme"></a>
# **GetMe**
> MeResponse GetMe ()

Get credentials

Retrieves the account information associated with the API key being used.


### Parameters
This endpoint does not need any parameter.
### Return type

[**MeResponse**](MeResponse.md)

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

