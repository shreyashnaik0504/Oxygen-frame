# OxygenFrame — Windows Desktop Photo Frame

A lightweight Windows 10/11 desktop photo-frame app with a separate, simple control center. Built with WPF and .NET 8; no third-party NuGet packages are required.

## Included features

### Floating photo frame
- Slideshow from a selected folder; watches the folder for new/removed images
- Smooth crossfade transitions
- Drag to move and resize the frame
- Rounded corners, adjustable border, image fit, opacity, shadow softness/strength/direction
- Optional neon glow, glass styling, and Polaroid style
- Optional filename label, pause-on-hover, and always-on-top mode

### Simple control center
- Choose or refresh the photo folder
- Previous / next photo buttons
- Pause / resume slideshow
- Adjustable slideshow interval
- Shuffle order and include subfolders
- Appearance and shadow controls
- Settings automatically saved to `%AppData%\OxygenFrame\settings.json`
- Keyboard shortcuts in the control center: `N` = next photo, `P` = previous photo, `Space` = pause/resume, `Esc` = close control center

## Run on Windows 10/11
1. Install the **.NET 8 SDK for Windows**.
2. Extract the ZIP.
3. Double-click `publish-windows.bat` to create a standalone build in the `publish` folder, or open a terminal here and run `dotnet run`.
4. Run `publish\OxygenFrame.exe` (after publishing) and choose a folder of photos.

## Important
This package contains the complete project source and a publish script, not a precompiled `.exe`. It must be built on Windows with the .NET 8 SDK. This environment does not have the Windows build toolchain, so the app could not be compiled or runtime-tested here.

Wallpaper-layer attachment is best-effort due to Windows desktop implementation differences. If that does not work on a particular system, the widget still runs as a normal floating desktop window.
