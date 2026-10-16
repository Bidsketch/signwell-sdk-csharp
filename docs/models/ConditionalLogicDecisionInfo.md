# SignWell.Sdk.Models.ConditionalLogicDecisionInfo
Audit record of a signer-side decision about a CL rule that fired against a completed field.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**RuleId** | **string** | External ID of the conditional rule that fired | 
**FieldApiId** | **string** | api_id of the field the decision applies to | 
**DecisionType** | **string** | Whether the signer kept the existing signature or the value was cleared | 
**DecidedAt** | **DateTimeOffset** | When the decision was recorded | 
**Description** | **string** | Human-readable summary of the decision | [optional] 

[[Back to Model list]](../../README.md#documentation-for-models) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to README]](../../README.md)

