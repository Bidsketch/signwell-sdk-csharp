# SignWell.Sdk.Models.ConditionalRuleInfo
Conditional rule configuration

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **string** | Conditional rule ID | 
**TargetId** | **string** | api_id of the field that owns the rule, or the external_id of a checkbox group when the rule is owned by a group (groups have no api_id) | 
**ActionType** | **string** | Action applied to action_field_ids when the conditions are met | 
**ActionFieldIds** | **List&lt;string&gt;** | api_ids of fields the rule affects when triggered | [optional] 
**Conditions** | [**List&lt;ConditionalRuleInfoConditionsInner&gt;**](ConditionalRuleInfoConditionsInner.md) |  | [optional] 

[[Back to Model list]](../../README.md#documentation-for-models) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to README]](../../README.md)

