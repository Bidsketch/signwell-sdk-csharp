# Changelog

## 0.2.0-beta.1 — Unreleased

### Added

- Support the current documented SMS delivery fields, conditional rules and decisions, and embedded signing/branding options.
- Expose 21 CFR Part 11 indicators in document responses and updated passcode-delivery settings.

### Changed

- Refresh generated resources, request/response models, and API documentation from the current published SignWell API.
- Regenerate with OpenAPI Generator 7.26.0 while preserving existing minimum runtimes.
- Standardize the default SDK User-Agent; callers can continue to configure their own value.
- Represent authenticated-user and active-user IDs as strings so legacy numeric IDs deserialize correctly. Membership, account, and contact IDs retain their UUID types.

### Dependencies

- Update Microsoft runtime packages, SourceLink, the test SDK, and xUnit to current compatible releases.
- Update Polly to 8.8 while retaining its supported v7 policy API.

### Fixed

- Accept missing or null email in SMS-only document and template-document recipient responses while preserving email-first constructor calls.
- Point repository metadata and future SDK syncs to `Bidsketch/signwell-sdk-csharp`.

- Serialize dropdown options as their string or detailed object values and read both forms correctly.
- Serialize passcode delivery methods with API wire values such as `email`; handle unknown standalone and nested enum values consistently.
- Preserve the original email-first recipient and template-recipient constructor signatures, including all optional positional arguments.
- Retry explicit HTTP 429 responses up to two times with `Retry-After` support and bounded exponential backoff. Cancellation stops retry waits; network failures and other status codes are not retried. Set `MaxRateLimitRetries` to zero to disable.

### Migration

- Code that explicitly uses `Guid` for user IDs should use `string`. This does not change membership, account, or contact ID types.

- Existing email-first `RecipientsInner` and `TemplateRecipientsInner` positional calls remain supported. SMS recipients can omit email using the new constructor and named arguments such as `name:`, `deliveryMethod:`, and `phoneNumber:`. Use named optional arguments when adding new delivery settings.
