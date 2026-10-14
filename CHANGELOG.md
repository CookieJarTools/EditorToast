# Changelog

All notable changes to this package are written here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this package follows
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.2.0] - 2026-10-14

### Added

- **An action on a toast, which is the thing that makes one worth raising.** `ToastAction` puts
  one button on it, usually an Undo or a way to see what it is talking about. A toast carrying an
  action never times out, because a way back that disappears while it is being read is worse than
  none at all.
- **Repeats collapse into one toast wearing a count** rather than a pile saying the same thing.
  Group them with `Key`, or let it match on the title and message.
- **The timer stops while the pointer is on it**, so a toast cannot leave in the middle of being
  read.
- **A readable default lifetime**, worked out from how much the toast says, with a floor of four
  seconds.
- `ToastManager.DismissAll`, and a `Success` severity.
- Examples under `Window > CookieJarTools > Editor Toast` showing the four shapes a toast is
  actually good for.

### Changed

- **The look.** A near-black card with a hairline round it, severity carried by a small tinted
  mark and a stripe down the edge instead of a block of colour across the top, the close button
  kept out of sight until the pointer is on it, and the time left shown as a hairline rather than
  a bar that asks to be watched. All of it now lives in a stylesheet rather than in inline styles.
- **A toast grows to fit what it says**, instead of being cut off by a fixed box.
- **No sound unless one is asked for.** `ToastManager.Sound` takes a clip. It used to load one
  from a hardcoded path inside the project it was installed into.
- The readme says what toasts are for, and where they are the wrong shape.

### Fixed

- **The package no longer only works on Windows.** Both the window placement and the rounded
  corners called into user32 and dwmapi with no platform check, so the first toast on macOS or
  Linux threw. Placement now comes from Unity, and the corner rounding is behind a Windows check.
- **Toast lifetimes are measured on editor time**, not `Time.time`, which does not advance
  dependably outside play mode.

## [1.1.1] - 2026-09-23

### Added

- The package, as it stands: toast notifications for the Unity editor.
