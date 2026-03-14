# Change Log

## [2.0.0]

### Breaking Changes

- Upgraded project dependencies and modernized the build system.
- Library version increased to 2.0.0 to reflect significant framework and dependency updates.
- Example project upgraded to `net10.0` and now uses the minimal API hosting model.

### Updates

- Adopted Central Package Management with `Directory.Packages.props`.
- Centralized build properties in `Directory.Build.props` and enabled `TreatWarningsAsErrors`.
- Updated all major package dependencies, including AWS SDK, `Microsoft.Extensions.Configuration`, `Newtonsoft.Json`, and `YamlDotNet`, to their latest versions.
- Reorganized project structure: moved example project to a dedicated `samples` directory.
- Added basic integration tests for JSON and YAML S3 file providers.
- Improved code quality with updated `.editorconfig` and general code style/formatting cleanup.

### Security Patches

- Upgraded `Newtonsoft.Json` to version 13.0.2 or later to address known security vulnerabilities.

## [1.2.0]

### Changes

- Modified reload routine to only reload if the configuration file actually changes. This increases performance
and reduces egress bandwidth.

## [1.0.1]

- Security fixes.

## [1.0.0]

Initial release.

- S3 File provider for JSON
- S3 File provider for YAML
