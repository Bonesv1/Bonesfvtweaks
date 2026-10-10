# Bones Tweaks — Full Panel (C# / .NET 8)

Updated native Windows app containing the original Windows, FPS, input latency, controller, and Pro registry settings, plus guidance-only cards where no safe universal tweak exists.

## Updating your GitHub repository
Replace `Program.cs` with the one in this ZIP, then commit it. Your existing `.github/workflows/build.yml` will automatically build the updated executable under **Actions**. Download the **BonesTweaks_Windows** artifact from the successful run.

This is a source project, not a compiled `.exe`. The native executable is built by GitHub Actions on Windows. Do not upload owner-only Pro private signing keys.

Registry changes are per-user, opt-in, and backed up to `%LOCALAPPDATA%\\BonesTweaks\\backup-v2.json`. Restore All restores saved values. Settings are not guaranteed to improve FPS or latency; some require signing out to take effect. This release does not change network drivers, USB polling rates, kernel timers, or Fortnite anti-cheat settings.

The older app's `backup.json` is retained untouched. If you applied settings using the old app, restore them with the old app first to avoid conflicting backups.
