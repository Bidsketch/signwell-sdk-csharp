using SignWell.Sdk.Client;

namespace SignWell.Sdk.Models;

public partial class RecipientsInner
{
    /// <summary>Preserves the email-first constructor used before SMS delivery was added.</summary>
    public RecipientsInner(string id, string email, Option<string?> name = default,
        Option<string?> passcode = default, Option<string?> subject = default,
        Option<string?> message = default, Option<bool?> sendEmail = default,
        Option<int?> sendEmailDelay = default)
        : this(id: id, email: new Option<string?>(email), name: name, passcode: passcode,
            subject: subject, message: message, sendEmail: sendEmail, sendEmailDelay: sendEmailDelay)
    { }
}

public partial class TemplateRecipientsInner
{
    /// <summary>Preserves the email-first constructor used before SMS delivery was added.</summary>
    public TemplateRecipientsInner(string id, string email, Option<string?> name = default,
        Option<string?> placeholderName = default, Option<string?> passcode = default,
        Option<string?> subject = default, Option<string?> message = default,
        Option<bool?> sendEmail = default, Option<int?> sendEmailDelay = default)
        : this(id: id, email: new Option<string?>(email), name: name, placeholderName: placeholderName,
            passcode: passcode, subject: subject, message: message, sendEmail: sendEmail,
            sendEmailDelay: sendEmailDelay)
    { }
}

public partial class DocumentResponseRecipientsInner
{
    /// <summary>Preserves the email-first constructor used before SMS-only responses were supported.</summary>
    public DocumentResponseRecipientsInner(string name, string email, Option<string?> id = default,
        Option<string?> body = default, Option<string?> message = default, Option<string?> subject = default,
        Option<bool?> sendEmail = default, Option<int?> sendEmailDelay = default,
        Option<DeliveryMethodEnum?> deliveryMethod = default, Option<DocumentResponseRecipientsInnerSms?> sms = default,
        Option<int?> signingOrder = default, Option<string?> signingUrl = default, Option<string?> embeddedSigningUrl = default,
        Option<bool?> bounced = default, Option<string?> bouncedDetails = default,
        Option<List<AttachmentRequestInfo>?> attachmentRequests = default, Option<string?> passcode = default,
        Option<DocumentResponseRecipientsInnerPasscodeDelivery?> passcodeDelivery = default, Option<string?> status = default)
        : this(name: name, email: new Option<string?>(email), id: id, body: body, message: message,
            subject: subject, sendEmail: sendEmail, sendEmailDelay: sendEmailDelay, deliveryMethod: deliveryMethod,
            sms: sms, signingOrder: signingOrder, signingUrl: signingUrl, embeddedSigningUrl: embeddedSigningUrl,
            bounced: bounced, bouncedDetails: bouncedDetails, attachmentRequests: attachmentRequests,
            passcode: passcode, passcodeDelivery: passcodeDelivery, status: status)
    { }
}

public partial class DocumentFromTemplateResponseRecipientsInner
{
    /// <summary>Preserves the email-first constructor used before SMS-only responses were supported.</summary>
    public DocumentFromTemplateResponseRecipientsInner(string email, Option<string?> id = default,
        Option<string?> name = default, Option<string?> role = default, Option<string?> message = default,
        Option<string?> subject = default, Option<bool?> sendEmail = default, Option<int?> sendEmailDelay = default,
        Option<DeliveryMethodEnum?> deliveryMethod = default, Option<DocumentResponseRecipientsInnerSms?> sms = default,
        Option<int?> signingOrder = default, Option<string?> signingUrl = default, Option<string?> embeddedSigningUrl = default,
        Option<bool?> bounced = default, Option<string?> bouncedDetails = default,
        Option<List<AttachmentRequestInfo>?> attachmentRequests = default, Option<string?> passcode = default,
        Option<DocumentResponseRecipientsInnerPasscodeDelivery?> passcodeDelivery = default,
        Option<string?> status = default, Option<string?> placeholderName = default)
        : this(email: new Option<string?>(email), id: id, name: name, role: role, message: message,
            subject: subject, sendEmail: sendEmail, sendEmailDelay: sendEmailDelay, deliveryMethod: deliveryMethod,
            sms: sms, signingOrder: signingOrder, signingUrl: signingUrl, embeddedSigningUrl: embeddedSigningUrl,
            bounced: bounced, bouncedDetails: bouncedDetails, attachmentRequests: attachmentRequests,
            passcode: passcode, passcodeDelivery: passcodeDelivery, status: status, placeholderName: placeholderName)
    { }
}
