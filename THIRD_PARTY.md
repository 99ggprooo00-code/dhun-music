# Third-party and upstream notices

## Nagi

DHUN Native is based on Nagi, Copyright its contributors, licensed under GPL-3.0.

- https://github.com/Anthonyy232/Nagi
- Base revision: `60f593ce1a54315fe1247d7fd0a3d89bdca768eb`

## Image processing change

Upstream Nagi 2.0.2 referenced SixLabors.ImageSharp 4.0.0, whose build requires a Six Labors license key. A clean fork build failed without the upstream maintainer's private key. DHUN replaced that dependency with:

- SkiaSharp 2.88.9 — MIT license

The replacement retains content hashing, cover resizing, JPEG cache output, color extraction, atomic writes and the existing image-processor interface.

## Other dependencies

NuGet dependencies and centrally pinned versions are declared in `Directory.Packages.props`. Each dependency remains under its own license. Before DHUN distribution, generated notices will be audited and included with the package.

## Feature research only

SimpMusic, Metrolist, ViMusic, Tauon and SimMusic 2024 may inform UX and source-boundary design. No source from those projects has been copied into this foundation branch at this stage.
