# SignWell.Sdk.Raw.WebhooksApi

All URIs are relative to *https://www.signwell.com*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**CreateWebhook**](WebhooksApi.md#createwebhook) | **POST** /api/v1/hooks | Create Webhook |
| [**DeleteWebhook**](WebhooksApi.md#deletewebhook) | **DELETE** /api/v1/hooks/{id} | Delete Webhook |
| [**ListWebhooks**](WebhooksApi.md#listwebhooks) | **GET** /api/v1/hooks | List Webhooks |

<a id="createwebhook"></a>
# **CreateWebhook**
> WebhookResponse CreateWebhook (CreateWebhookRequest createWebhookRequest)

Create Webhook

Register a callback URL that we will post document events to.


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **createWebhookRequest** | [**CreateWebhookRequest**](CreateWebhookRequest.md) |  |  |

### Return type

[**WebhookResponse**](WebhookResponse.md)

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
| **429** | rate limit exceeded |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="deletewebhook"></a>
# **DeleteWebhook**
> void DeleteWebhook (Guid id)

Delete Webhook

Deletes a registered callback URL that we are posting document events to.


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

<a id="listwebhooks"></a>
# **ListWebhooks**
> List&lt;WebhookResponse&gt; ListWebhooks ()

List Webhooks

List all the webhooks in the account.


### Parameters
This endpoint does not need any parameter.
### Return type

[**List&lt;WebhookResponse&gt;**](WebhookResponse.md)

### Authorization

[api_key](../README.md#api_key)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | ok |  -  |
| **429** | rate limit exceeded |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

