using MimeKit;

namespace Email.Configuration
{
    /// <summary>
    /// Validates reusable email content and delivery policy settings.
    /// </summary>
    public static class EmailDeliveryPolicyOptionsValidator
    {
        /// <summary>
        /// Adds validation failures for the supplied delivery policy.
        /// </summary>
        /// <param name="policy">The policy to validate.</param>
        /// <param name="path">The configuration path used in failure messages.</param>
        /// <param name="errors">The collection that receives validation failures.</param>
        public static void Validate(EmailDeliveryPolicyOptions policy, string path, ICollection<string> errors)
        {
            ArgumentNullException.ThrowIfNull(policy);
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            ArgumentNullException.ThrowIfNull(errors);

            if (policy.MaxRecipients <= 0)
            {
                errors.Add($"{path}:MaxRecipients must be greater than zero.");
            }

            if (policy.MaxAttachmentCount < 0)
            {
                errors.Add($"{path}:MaxAttachmentCount cannot be negative.");
            }

            if (policy.MaxAttachmentBytes <= 0)
            {
                errors.Add($"{path}:MaxAttachmentBytes must be greater than zero.");
            }

            if (policy.MaxTotalAttachmentBytes <= 0)
            {
                errors.Add($"{path}:MaxTotalAttachmentBytes must be greater than zero.");
            }

            if (policy.MaxTotalAttachmentBytes < policy.MaxAttachmentBytes)
            {
                errors.Add($"{path}:MaxTotalAttachmentBytes cannot be less than MaxAttachmentBytes.");
            }

            if (policy.MaxMessageBytes <= 0)
            {
                errors.Add($"{path}:MaxMessageBytes must be greater than zero.");
            }

            ValidateContentTypes(policy.AllowedAttachmentContentTypes, path, errors);
            ValidateExtensions(policy.AllowedAttachmentExtensions, path, errors);

            if (policy.AllowedSenderDomains is null || policy.AllowedSenderDomains.Any(string.IsNullOrWhiteSpace))
            {
                errors.Add($"{path}:AllowedSenderDomains cannot contain empty values.");
            }
        }

        private static void ValidateContentTypes(string[]? contentTypes, string path, ICollection<string> errors)
        {
            if (contentTypes is null || contentTypes.Any(string.IsNullOrWhiteSpace))
            {
                errors.Add($"{path}:AllowedAttachmentContentTypes cannot contain empty values.");
                return;
            }

            foreach (string contentType in contentTypes)
            {
                if (!IsValidContentTypePattern(contentType))
                {
                    errors.Add($"{path}:AllowedAttachmentContentTypes contains invalid MIME type '{contentType}'.");
                }
            }
        }

        private static void ValidateExtensions(string[]? extensions, string path, ICollection<string> errors)
        {
            if (extensions is null || extensions.Any(string.IsNullOrWhiteSpace))
            {
                errors.Add($"{path}:AllowedAttachmentExtensions cannot contain empty values.");
                return;
            }

            foreach (string extension in extensions)
            {
                string normalized = extension.Trim();
                if (normalized.Length <= 1 || normalized[0] != '.' || normalized.Any(char.IsWhiteSpace) || normalized.IndexOfAny(['/', '\\']) >= 0)
                {
                    errors.Add($"{path}:AllowedAttachmentExtensions contains invalid extension '{extension}'.");
                }
            }
        }

        private static bool IsValidContentTypePattern(string contentType)
        {
            string normalized = contentType.Trim();
            int separatorIndex = normalized.IndexOf('/');

            if (separatorIndex <= 0 || separatorIndex != normalized.LastIndexOf('/') || separatorIndex == normalized.Length - 1 || normalized.Contains(';'))
            {
                return false;
            }

            string mediaType = normalized[..separatorIndex];
            string mediaSubtype = normalized[(separatorIndex + 1)..];

            if (mediaType == "*" && mediaSubtype != "*")
            {
                return false;
            }

            return mediaType == "*" && mediaSubtype == "*" ||
                mediaSubtype == "*"
                    ? ContentType.TryParse($"{mediaType}/octet-stream", out _)
                    : ContentType.TryParse(normalized, out _);
        }
    }
}
