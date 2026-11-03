# Security policy

## Supported versions

Security fixes are applied to the current default branch and released as new
package versions. Consumers should update deliberately and retain the source
commit, dependency audit, test results, package checksums and approval record.

## Reporting a vulnerability

Do not open a public issue for suspected vulnerabilities or include credentials,
tokens, connection strings, personal data or exploit details in an issue. Use
GitHub's private vulnerability reporting for this repository:

<https://github.com/izrrachgz/command-q/security/advisories/new>

Include the affected package/version and API, impact, safe reproduction steps and
any known mitigations. Maintainers should acknowledge receipt, assess affected
consumers, establish remediation ownership and coordinate disclosure after fixed
packages are available.

## Package incident response

For a compromised or defective package, stop distribution, preserve the source
commit and build evidence, notify package-feed and consuming-application owners,
publish a corrected version, and document whether credentials or personal data
could have been exposed. NuGet versions are immutable: do not replace an existing
version with different content.
