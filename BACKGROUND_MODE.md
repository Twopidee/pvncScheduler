# Background Mode Guide

Your PostgreSQL Backup Manager now runs as a background service with system tray integration!

## How To Use Background Mode

### Minimize to System Tray

1. **Start the application normally**
2. **Click the minimize button** (—) or resize the window
   - The window will hide
   - An icon will appear in your system tray (bottom-right corner, next to the clock)

### Control from System Tray

**Left-click the tray icon:**
- Toggle the window visibility
- Show/hide the application window

**Right-click the tray icon:**
- **Show** - Display the window
- **Backup Now** - Run a backup immediately (even with closed window!)
- **Exit** - Close the application completely

## Scheduled Backups in Background

**Key Feature:** Your scheduled backups will continue running even when the window is minimized or hidden!

### To Enable Background Backups:

1. Open the application normally
2. Configure your:
   - Database (pvnc)
   - Backup folder
   - Schedule (Daily/Weekly/Monthly)
   - Backup time
3. Check the "Enabled" checkbox
4. Minimize the window to tray
5. **Backups will now run automatically at the scheduled time**

### Status While in Background

Even when minimized, you can:
- Right-click the tray icon and select **"Show"** to check status
- View recent backups
- See the scheduled backup time
- Trigger manual backups anytime

## Running on System Startup

To make the app start automatically when you boot:

### Option 1: Windows Startup Folder (Recommended)

1. Press **Win + R**
2. Type: `shell:startup`
3. Press Enter (folder opens)
4. Create a **shortcut** to `pvncScheduler.exe` and paste it here
5. The app will launch automatically on next boot

### Option 2: Task Scheduler

1. Press **Win + R**
2. Type: `taskschd.msc`
3. Click "Create Basic Task" on the right
4. Name it: "PostgreSQL Backup Manager"
5. Choose trigger: "At log on"
6. Choose action: Start a program
7. Browse to: `C:\VisualBasicPrograms\pvncScheduler\bin\Debug\net10.0-windows\pvncScheduler.exe`
8. Check "Run with highest privileges"
9. Finish

## Tips for Best Results

1. **Always have backup enabled on startup:**
   - Set your schedule before starting on boot
   - Settings are auto-stored

2. **Keep your PC running:**
   - Scheduled backups only run when the PC is powered on
   - If your PC is off at scheduled time, the backup is skipped

3. **Monitor backups:**
   - Occasionally check the tray icon and open the window
   - Verify recent backups are being created
   - Check status messages

4. **Backup retention:**
   - Only 10 most recent backups are kept
   - Older backups are automatically deleted
   - This saves disk space automatically

## Example Workflow

**Setting up for 24/7 Background Operation:**

1. Configure backup folder: `C:\Backups\PostgreSQL`
2. Set schedule: "Daily"
3. Set time: "02:00" (2 AM)
4. Check "Enabled"
5. Right-click the tray icon and select "Backup Now" to test
6. Minimize the window to tray
7. Close the window using the X button (app stays in tray)
8. Add to Startup folder so it launches automatically each boot
9. Scheduled backups now run daily at 2 AM automatically!

## Troubleshooting Background Mode

**The window won't minimize:**
- Click the minus button (—) on the title bar

**Icon doesn't appear in tray:**
- Check the "Show hidden icons" arrow in the bottom-right corner
- The icon may be in the hidden tray area

**Scheduled backup didn't run:**
- Open the window and check "Scheduled Backup" status
- Verify the schedule is enabled
- If PC was off at scheduled time, check next backup time
- Run "Backup Now" manually to test

**I want to close the app:**
- Right-click the tray icon and select "Exit"
- Do NOT just close the window (that just minimizes it!)

## Icon Color Indicators

The tray icon shows:
- **Available** - App is running and ready
- **Left-click to show/hide** - Toggle window visibility
- **Right-click for menu** - Access backup controls

## Keyboard Shortcut

When the window is open, you can Alt+Tab to switch to it like any window.
