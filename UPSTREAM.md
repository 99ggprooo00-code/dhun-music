# Upstream provenance

DHUN Native is forked from:

- Project: Nagi
- Repository: https://github.com/Anthonyy232/Nagi
- Upstream revision: `60f593ce1a54315fe1247d7fd0a3d89bdca768eb`
- License: GNU General Public License v3.0

## Initial DHUN changes

- Added a fork-safe Windows verification workflow.
- Replaced the build-key-enforced SixLabors.ImageSharp 4 dependency with MIT-licensed SkiaSharp.
- Preserved the upstream `IImageProcessor` contract and image cache behavior.
- Replaced visible Nagi product branding and Windows package identity with DHUN branding.
- Initially reset the inherited upstream version to DHUN `0.1.0`; the native preview was later designated `v1.0.0` by the project owner.
- Removed upstream Azure deployment, signing, benchmarking, Crowdin, sponsorship media and release automation that depend on the upstream maintainer's infrastructure.
- Retained internal `Nagi.*` namespaces temporarily so rebranding can proceed in tested stages instead of one dangerous global rename.

Upstream copyright notices and GPL rights remain intact in repository history and source files.
