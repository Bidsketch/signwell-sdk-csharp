# SignWell.Sdk.Models.EmbeddedRequest
Optional container for embedded settings. Purely additive: requests that omit it behave exactly as before, and the existing top-level `api_application_id`, `embedded_signing` and `embedded_signing_notifications` parameters are unchanged. Not supported on template endpoints, which return a `422` if it is present.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Branding** | [**EmbeddedBrandingRequest**](EmbeddedBrandingRequest.md) |  | [optional] 

[[Back to Model list]](../../README.md#documentation-for-models) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to README]](../../README.md)

