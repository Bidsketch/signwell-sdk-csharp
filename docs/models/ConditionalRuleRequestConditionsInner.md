# SignWell.Sdk.Models.ConditionalRuleRequestConditionsInner

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**ConditionType** | **string** | How the field_api_ids combine within a condition: &#x60;any_of&#x60;, &#x60;all_of&#x60;, &#x60;none_of&#x60;, or &#x60;not_all_of&#x60;. | 
**Status** | **string** | The field state the condition tests for: &#x60;completed&#x60; or &#x60;not_completed&#x60;. | 
**FieldApiIds** | **List&lt;string&gt;** | api_ids of the trigger fields the condition watches. | 
**JoinOperator** | **string** | How this condition joins the next one in the rule: &#x60;and_operator&#x60; or &#x60;or_operator&#x60;. The first condition&#39;s value is ignored. | [optional] 
**Position** | **int** | 1-indexed order of the condition within the rule. | [optional] 

[[Back to Model list]](../../README.md#documentation-for-models) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to README]](../../README.md)

