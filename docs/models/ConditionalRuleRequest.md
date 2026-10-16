# SignWell.Sdk.Models.ConditionalRuleRequest
A conditional logic rule. Create requires target_id, action_type, action_field_ids and conditions; id and deleted are ignored on create. On an update (template update or document update-and-send) the rules are an incremental upsert keyed by id: send id + the fields to change to modify a rule, id + deleted: true to remove it, or omit id to add a new rule.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **string** | The rule&#39;s persistent ID. Ignored on create (every rule is new). On an update (template update or document update-and-send) include it to modify an existing rule, or pair it with &#x60;deleted: true&#x60; to remove the rule; omit it to add a new rule. | [optional] 
**Deleted** | **bool** | Only meaningful on an update (template update or document update-and-send): set to true together with the rule&#39;s &#x60;id&#x60; to remove that rule. Ignored on create — there is no existing rule to delete. | [optional] 
**TargetId** | **string** | api_id of the field that owns the rule, or the external_id of a checkbox group when the rule is owned by a group (groups have no api_id). Cannot also appear in action_field_ids (self-reference is forbidden). | [optional] 
**ActionType** | **string** | Action applied to action_field_ids when the conditions are met: &#x60;show&#x60;, &#x60;hide&#x60;, or &#x60;require&#x60;. | [optional] 
**ActionFieldIds** | **List&lt;string&gt;** | api_ids of the fields the action applies to. List a checkbox group&#39;s member api_ids to target the whole group. | [optional] 
**Conditions** | [**List&lt;ConditionalRuleRequestConditionsInner&gt;**](ConditionalRuleRequestConditionsInner.md) | The conditions that trigger the rule. At least 1, at most 5. | [optional] 

[[Back to Model list]](../../README.md#documentation-for-models) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to README]](../../README.md)

