# Install Unity
Get Unity 2019.3.0f6: https://unity.com/releases/editor/whats-new/2019.3.0f6
I recommend using one of the Download Assistances since it lets you pick what pieces to install.
The parts that are supported for now are:
- Android Build Support
- Mac Build Support
- Windows Build Support

## Additional Android Packages
### JDK
You'll also need a java JDK (recommend OpenJDK 8 (1.8)): https://adoptium.net/
Use Unity's menu Edit -> Preferences -> Exteranl Tools and set the JDK to point to the place where you installed/unzipped the JDK.

### SDK
Get an Android SDK that supports platforms;android-29 and build-tools;29.0.3 from somewhere on android.com: https://developer.android.com/studio#command-tools
You can install the command-line tools from there and use sdkmanager to download these.
Use Unity's menu Edit -> Preferences -> Exteranl Tools and set the Android SDK to point to the place where you installed/unzipped the SDK.

### NDK
For ARM64 native builds for Android, you'll also need to download NDK r19: https://dl.google.com/android/repository/android-ndk-r19-windows-x86_64.zip
Unzip that somewhere.
Use Unity's menu Edit -> Preferences -> External Tools and set the Android NDK to point at the directory where you unzipped the NDK.

## Build Settings
### Windows

### Mac


### Android
In Project Settings -> Android
Resolution and Presentation:
Start in fullscreen mode: checked
Render outside safe area: checked
Resolution Scaling Mode: Disabled
Blit Type: Always
Supported Aspect Ratio: Native Aspect Ratio
Default Orientation: Auto Rotation
Allowed Orientations for Auto Rotation:
	Landscape Right: checked
	Landscape Left: checked
Use 32-bit Display Buffer: checked

Other Settings:
Color Space: Linear
Multithreaded Rendering: checked
Static Batching: checked
Compute Skinning: checked
Lightmap Encoding: Low Quality
Lightmap Streaming Enabled: checked
	Streaming Priority: 0
Vulkan Settings: 
Number of swapchain buffers: 3
Identification:
Package Name: com.GlowPuff.YourJourney
Version: 0.37 (update as needed)
Bundle Version Code: 1
Minimum API Level: Android 4.4 (API level 19)
Target API Level: API level 29
Configuration:
Scripting Backend: IL2CPP (This is needed for Arm64. Otherwise you can use Mono for just ARMv7.)
Api Compatibility Level: .NET Standard 2.0
C++ Compiler Configuration: Release
Target Architectures
	AMRv7: checked
	ARM64: checked (for newer devices like Pixel 9 with native code required instead of JIT compilation)
Install Location: Prefer External
Internet Access: Auto
Write Permission: External (SDCard)
Scripting Define Symbols: UNITY_POST_PROCESSING_STACK_V2
Active Input Handling: Input Manager (Old)
Optimization:
Strip Engine Code: Checked
Managed Stripping Level: Low
Vertex Compression: Mixed... (Normal, Tangent, Tex Coord 0, Tex Coord 2, Tex Coord 3)
Optimize Mesh Data: checked