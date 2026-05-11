# Bite Check Android Debug Build

## One-Time Setup

1. Install Unity Android Build Support, Android SDK, Android NDK, and OpenJDK from Unity Hub.
2. Open the project in Unity.
3. Run `Tools > Bite Check > Apply Android Portrait Settings`.
4. Run `Tools > Bite Check > Create Prototype Scene`.
5. Open `File > Build Settings`.
6. Select `Android`, then click `Switch Platform` if needed.
7. Add `Assets/_BiteCheck/Scenes/MainScene.unity` to `Scenes In Build`.

## Player Settings Checklist

- Resolution and Presentation:
  - Default Orientation: `Portrait`
  - Auto Rotation: only Portrait enabled if using autorotation
- Other Settings:
  - Package Name: `com.zozi.bitecheck`
  - Minimum API Level: Android 6.0 or newer
  - Target API Level: Automatic or latest installed
- Publishing Settings:
  - Debug builds can use Unity's default debug signing.
  - Release builds need a real keystore later.

## Debug Build

1. Connect an Android device with USB debugging enabled, or start an Android emulator.
2. Open `File > Build Settings`.
3. Enable `Development Build`.
4. Enable `Script Debugging` if you need debugger attachment.
5. Click `Build And Run`.
6. Save the APK as `Builds/Android/BiteCheck-debug.apk`.

## Quick Device Test

- The game should open in portrait.
- The main menu should fit inside the safe area.
- Tap `START`.
- Swipe left and right on the touchscreen.
- Finish a day and verify the summary and `CONTINUE` button.
- Force enough wrong decisions to verify the game over and `RESTART` flow.

## Notes

- Ads, IAP, accounts, and backend services are intentionally not configured.
- This is still a prototype build; performance, art, and final store settings are not production-ready.
