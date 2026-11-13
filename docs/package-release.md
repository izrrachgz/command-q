# Secure package release

The five `CommandQ.*` packages are reusable build dependencies. They are
intentionally unobfuscated: public API inspection, Source Link, portable symbols,
debugging and compatibility analysis are part of the package contract. Compiled
assemblies must never contain secrets. Obfuscation is not a release step and the
repository convention tests prevent it from being introduced accidentally.

## Build evidence

Run from a reviewed commit with the SDK selected by `global.json`:

```powershell
.\scripts\build.ps1 -Configuration Release -LockedMode -Audit
.\scripts\test.ps1 -Configuration Release -NoBuild
```

The build restores only versions recorded by committed lock files, treats NuGet
advisories as errors, compiles deterministic assemblies, runs package validation,
and produces one `.nupkg` plus one `.snupkg` per library. The verifier checks the
package identity, meaningful description, README, repository URL/commit, expected
assembly and portable PDB. It writes `SHA256SUMS` over all ten artifacts.

CI verifies packages without uploading or distributing them. Before an authorized
distribution, record:

1. Source commit, package version and change approval.
2. Successful audit, Release build, tests and package-verification output.
3. SHA-256 manifest and destination feed.
4. Compatibility review and documented breaking changes.
5. Publisher identity and distribution approval.

## Integrity and signing

The local sibling feed relies on repository access controls, CI verification and
the generated SHA-256 manifest. If packages are distributed outside the controlled
development environment, the release owner must approve the destination and
signing policy. Where author signing is required, use an organization-controlled
code-signing certificate and RFC 3161 timestamp, verify signatures before upload,
and keep private keys only in the release secret/HSM service. Never commit signing
credentials.

An owner must also approve licensing terms before external distribution. This
repository and the generated `CommandQ.*` packages are licensed under the MIT
License, declared in the root [`LICENSE`](../LICENSE) file and package metadata.
Release reviewers must still verify that all bundled dependencies permit the
intended distribution model.

## Version and recovery rules

Package versions are immutable. Never overwrite or republish different bytes for
an existing version. If a release is defective or compromised, restrict/unlist it
where supported, publish a corrected incremented version, notify consumers and
retain the incident, revocation and remediation evidence.
