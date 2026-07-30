# SignWell.Sdk.Models.DocumentRequest

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Files** | [**List&lt;FilesInner&gt;**](FilesInner.md) | Document files can be uploaded by specifying a file URL or base64 string. Either &#x60;file_url&#x60; or &#x60;file_base64&#x60; must be present (not both). Valid file types are: .pdf, .doc, .docx, .pages, .ppt, .pptx, .key, .xls, .xlsx, .numbers, .jpg, .jpeg, .png, .tiff, .tif, .webp, .html, and .htm | 
**Recipients** | [**List&lt;RecipientsInner&gt;**](RecipientsInner.md) | Document recipients are people that must complete and/or sign a document. | 
**TestMode** | **bool** | Set to &#x60;true&#x60; to enable Test Mode. Documents created with Test Mode do not count towards API billing and are not legally binding. Defaults to &#x60;false&#x60; | [optional] [default to false]
**Name** | **string** | The name of the document. | [optional] 
**Subject** | **string** | Email subject for the signature request that recipients will see. Defaults to the default system subject or a template subject (if the document is created from a template). | [optional] 
**Message** | **string** | Email message for the signature request that recipients will see. Defaults to the default system message or a template message (if the document is created from a template). | [optional] 
**Draft** | **bool** | Whether the document can still be updated before sending a signature request. If set to &#x60;false&#x60; the document is sent for signing as part of this request. Defaults to &#x60;false&#x60;. | [optional] [default to false]
**SelfSign** | **bool** | When set to true the document will enter self-sign mode, which allows the document creator to sign and complete a document without any field setup. Must use the embedded_edit_url in the response and (if embedding) the embedded requesting workflow. | [optional] [default to false]
**WithSignaturePage** | **bool** | When set to &#x60;true&#x60; the document will have a signature page added to the end, and all signers will be required to add their signature on that page. | [optional] [default to false]
**ExpiresIn** | **int** | Number of days before the signature request expires. Defaults to the account expiration setting or template expiration (if the document is created from a template). | [optional] 
**Reminders** | **bool** | Whether to send signing reminders to recipients. Reminders are sent on day 3, day 6, and day 10 if set to &#x60;true&#x60;. Defaults to &#x60;true&#x60;. | [optional] [default to true]
**ApplySigningOrder** | **bool** | When set to &#x60;true&#x60; recipients will sign one at a time in the order of the &#x60;recipients&#x60; collection of this request. | [optional] [default to false]
**ApiApplicationId** | **Guid** | Unique identifier for API Application settings to use. API Applications are optional and mainly used when isolating OAuth apps or for more control over embedded API settings | [optional] 
**EmbeddedSigning** | **bool** | When set to &#x60;true&#x60; it enables embedded signing in your website/web application. Embedded functionality works with an iFrame and email authentication is disabled. :embedded_signinig defaults to &#x60;false&#x60;. | [optional] [default to false]
**EmbeddedSigningNotifications** | **bool** | On embedding signing, document owners (and CC&#39;d contacts) do not get a notification email when documents have been completed. Setting this param to &#x60;true&#x60; will send out those final completed notifications. Default is &#x60;false&#x60; | [optional] [default to false]
**TextTags** | **bool** | An alternative way (if you can’t use the recommended way) of placing fields in specific locations of your document by using special text tags. Useful when changing the content of your files changes the location of fields. See API documentation for “Text Tags” for details. Defaults to false. | [optional] [default to false]
**CustomRequesterName** | **string** | Sets the custom requester name for the document. When set, this is the name used for all email communications, signing notifications, and in the audit file. | [optional] 
**CustomRequesterEmail** | **string** | Sets the custom requester email for the document. When set, this is the email used for all email communications, signing notifications, and in the audit file. | [optional] 
**RedirectUrl** | **string** | A URL that recipients are redirected to after successfully signing a document. | [optional] 
**AllowDecline** | **bool** | Whether to allow recipients the option to decline signing a document. If multiple signers are involved in a document, any single recipient can cancel the entire document signing process by declining to sign. | [optional] [default to true]
**AllowReassign** | **bool** | In some cases a signer is not the right person to sign and may need to reassign their signing responsibilities to another person. This feature allows them to reassign the document to someone else. | [optional] [default to true]
**DeclineRedirectUrl** | **string** | A URL that recipients are redirected to if the document is declined. | [optional] 
**Language** | **string** | Sets the language for all recipients on the document and updates all recipient side interactions including the document email and the document itself. Accepted languages: English, Français, Español, Deutsch, Polski, Português, Dansk, Nederlands, Italiano, Русский, Svenska, العربية, Ελληνικά, Türkçe, Slovenčina. Defaults to English. Language should be sent in ISO 639-1 format: en, fr, es, de, pl, pt, da, nl, it, ru, sv, ar, el, tr, sk. | [optional] 
**Metadata** | **Dictionary&lt;string, string&gt;** | Optional key-value data that can be associated with the document. If set, will be available every time the document data is returned. | [optional] 
**Fields** | **List&lt;List&lt;FieldsInnerInner&gt;&gt;** | Document fields placed on a document for collecting data or signatures from recipients. At least one field must be present in the Create Document request if &#x60;draft&#x60; is &#x60;false&#x60; (unless adding a signature page by using &#x60;with_signature_page&#x60;). Field data should be sent as a 2-dimensional JSON array. One array of fields is needed for each file in the files array. An array of fields can be empty if you have a file that does not contain any fields. | [optional] 
**AttachmentRequests** | [**List&lt;AttachmentRequestsInner&gt;**](AttachmentRequestsInner.md) | Attachments that a recipient must upload to complete the signing process. Attachment requests are shown after all document fields have been completed. | [optional] 
**CopiedContacts** | [**List&lt;CopiedContactsInner&gt;**](CopiedContactsInner.md) | Copied contacts are emailed the final document once it has been completed by all recipients. | [optional] 
**Labels** | [**List&lt;LabelRequest&gt;**](LabelRequest.md) | Labels can be used to organize documents in a way that can make it easy to find using the document search in SignWell. A document can have multiple labels. | [optional] 
**CheckboxGroups** | [**List&lt;CheckboxGroupsInner&gt;**](CheckboxGroupsInner.md) | Checkbox fields that are placed on a document can be grouped with selection requirements. At least 2 checkbox fields in an array of fields must be assigned to the same recipient. | [optional] 

[[Back to Model list]](../../README.md#documentation-for-models) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to README]](../../README.md)

