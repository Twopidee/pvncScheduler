# Troubleshooting Guide: PostgreSQL Backup Failed

If you're seeing "Backup failed. Ensure pg_dump is installed and accessible", follow these steps:

## Step 1: Use the Diagnostics Tool (Easiest)

1. Click the **"Diagnostics"** button in the main window
2. The tool will automatically:
   - Check if pg_dump is installed
   - Show you the location of pg_dump
   - Verify your PostgreSQL connection

This is the fastest way to identify the problem!

---

## Step 2: Manual Troubleshooting

### Issue: pg_dump not found

If the diagnostics show pg_dump is not installed:

**Option A: PostgreSQL is not installed**
- Download PostgreSQL from: https://www.postgresql.org/download/windows/
- Choose version 9.2 or higher (15+ recommended)
- **During installation, make sure to check "Command Line Tools"**
- Complete the installation

**Option B: PostgreSQL is installed but not in PATH**
- Find your PostgreSQL installation directory
- Common locations:
  - `C:\Program Files\PostgreSQL\15\bin`
  - `C:\Program Files\PostgreSQL\14\bin`
  - `C:\Program Files\PostgreSQL\13\bin`
  - `C:\Program Files (x86)\PostgreSQL\*\bin`

**Add PostgreSQL bin folder to Windows PATH:**

1. Press `Win + X` and select "System"
2. Click "Advanced system settings"
3. Click "Environment Variables..." button
4. Under "System variables", select "Path" and click "Edit..."
5. Click "New" and add your PostgreSQL bin folder path
   - Example: `C:\Program Files\PostgreSQL\15\bin`
6. Click OK on all dialogs
7. **Restart the application**

---

### Issue: Connection failed (postgres user password)

If the diagnostics report a connection error:

**The program connects with:**
- Host: localhost
- User: postgres
- Password: whatever is entered in the **Password** box in the main window

**To fix a password error** (`password authentication failed` or `no password supplied`):

1. Type the correct `postgres` password into the **Password** box. It's saved automatically, encrypted for your Windows account.
2. Open **Diagnostics → Test Connection** to confirm it works.

Alternatively, leave the box empty and put the password in a [`pgpass.conf`](https://www.postgresql.org/docs/current/libpq-pgpass.html) file at `%APPDATA%\postgresql\pgpass.conf`:
```
localhost:*:*:postgres:<password>
```

---

### Issue: PostgreSQL is not running

Make sure PostgreSQL service is running:

1. Press `Win + R`
2. Type `services.msc`
3. Look for "postgresql" service
4. If stopped, right-click and select "Start"

---

## Step 3: Verify Everything Manually

Open Command Prompt and test directly:

1. Press `Win + R`
2. Type `cmd` and press Enter
3. Run: `pg_dump --version`
   - Should show PostgreSQL version
   - If not found, reinstall PostgreSQL or add to PATH

4. Run: `pg_dump -h localhost -U postgres -F c pvnc > test.dump`
   - Replace `pvnc` with your database name
   - This should create a file called `test.dump`
   - If it fails, diagnose the error message

---

## Step 4: Database Specific Issues

**Database doesn't exist:**
- Ensure "pvnc" database exists in PostgreSQL
- Run from Command Prompt:
  ```
  psql -h localhost -U postgres -c "\l"
  ```
  - This lists all databases
  - If "pvnc" is missing, create it

**User doesn't have permissions:**
- Run from PostgreSQL command line:
  ```
  psql -h localhost -U postgres
  GRANT ALL PRIVILEGES ON DATABASE pvnc TO postgres;
  ```

---

## Step 5: Advanced Debugging

Enable debug output by:

1. Open Visual Studio
2. Build → Configuration Manager
3. Set to "Debug" mode
4. Run the application
5. When you get the error, check the Output window (Debug output)
6. Copy any error messages and search PostgreSQL documentation

---

## Quick Checklist

- [ ] PostgreSQL is installed (version 9.2+)
- [ ] pg_dump is in system PATH (test: `pg_dump --version`)
- [ ] PostgreSQL service is running
- [ ] Database "pvnc" exists
- [ ] "postgres" user can connect without password
- [ ] Backup folder has write permissions
- [ ] No special characters in folder path

---

## Still Having Issues?

1. Run the **Diagnostics** tool and note all error messages
2. Check PostgreSQL logs:
   - Location: `C:\Program Files\PostgreSQL\15\data\log\`
3. Try manual backup from command line:
   ```
   pg_dump -h localhost -U postgres -F c pvnc > C:\backup\pvnc.dump
   ```
4. If that works, the issue is in the application configuration
5. If that fails, the issue is with PostgreSQL setup

---

## Getting Help

When asking for help, provide:
1. Output from Diagnostics tool
2. Output from manual `pg_dump --version` command
3. PostgreSQL version installed
4. Windows version
5. Any error messages from the application
