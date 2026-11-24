# Agent notes

A .NET 10 MAUI app (Android + iOS) that teaches foundation stage children to
halve groups of food pictures. Two games: `CircleHalfPage` (ring half of a
group) and `CutInHalfPage` (draw one line through the middle of a snack).

## Build and test

```bash
# Android (works on Linux)
dotnet build src/HalvingFun.csproj -f net10.0-android \
  -p:TargetFrameworks=net10.0-android -p:AndroidSdkDirectory=/workspace/android-sdk

dotnet test tests/HalvingFun.Tests
```

* `-p:TargetFrameworks=net10.0-android` is required off macOS. Without it,
  restore evaluates `net10.0-ios` too and fails with NETSDK1178 because the iOS
  workload cannot be installed on Linux (`dotnet workload install ios` refuses
  to run there).
* To type check `src/Platforms/iOS` without a Mac, compile those files in a
  throwaway `net10.0` project that references the `Microsoft.iOS.Ref.net10.0_26.0`
  NuGet package directly.
* The Android SDK, JDK 21 and the .NET SDK live outside the repo in this sandbox
  (`/workspace/android-sdk`, `/workspace/dotnet`); set `PATH` and `JAVA_HOME`
  before building.

## Shape of the code

* `Models/StrokeGeometry` - the marking rules: even-odd ray casting to see which
  pictures a ring encloses, shoelace area to ignore scribbles, and `IsFairCut`
  for the straight line game.
* `Drawing/ItemLayout` - the single source of truth for where pictures sit. The
  `Image` grid and the `GraphicsView` ink layer both use it, so pen lines always
  line up with the pictures. Change it in one place only.
* `Drawing/InkDrawable` - draws pen strokes, the green/red feedback rings and the
  half tint. It only depends on `Microsoft.Maui.Graphics`, which is why the tests
  and offline renders can use it without a device.
* `Services/SoundPlayerBase` copies bundled WAVs to the cache once, then hands a
  file path to the platform player (`MediaPlayer` on Android, `AVAudioPlayer` on
  iOS).
* `tests/HalvingFun.Tests` links the platform neutral sources instead of
  duplicating them; keep new marking or layout code free of `Microsoft.Maui.Controls`
  types so it stays testable. `InkDrawableTests` renders the ink layer through
  `Microsoft.Maui.Graphics.Skia`, which is the closest we can get to on-device
  drawing without an emulator.

## Conventions

* Everything a child sees is big, rounded and colourful; use the styles in
  `src/Resources/Styles` rather than inline colours.
* Every question is also spoken (`VoiceGuide`) because the players cannot read.
* A wrong answer never blocks the child: the hint appears, then the pen unlocks.
* Sounds and pictures must never throw. All platform audio calls are wrapped.
