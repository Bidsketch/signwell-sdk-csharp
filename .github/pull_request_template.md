## Summary

Describe the behavior changed and why.

## Validation

- [ ] `dotnet format --verify-no-changes`
- [ ] `dotnet test -c Release --locked-mode`
- [ ] Package/API compatibility checks pass
- [ ] Generated changes originate in `signwell-sdk-generator`

## Security

- [ ] No API keys, request payloads, response bodies, or sensitive query strings are logged
- [ ] New dependencies and generated package contents were reviewed
