Imports System.IO
Imports System.Diagnostics
Imports System.ComponentModel

Public Class Form1
    Private backupManager As PostgreSQLBackupManager
    Private scheduleManager As BackupScheduleManager
    Private settingsFile As String
    Private isExiting As Boolean = False
    Private startHidden As Boolean = False

    Private Const MinimizedArgument As String = "--minimized"
    Private Const StartupRegistryKey As String = "Software\Microsoft\Windows\CurrentVersion\Run"
    Private Const StartupValueName As String = "pvncScheduler"
    Private loadingStartupSetting As Boolean = False
    Friend Const LastDayOfMonth As String = "Last day"
    Private suppressScheduleChange As Boolean = False
    Private loadingSettings As Boolean = False

    Public Sub New()
        InitializeComponent()
        backupManager = New PostgreSQLBackupManager()
        scheduleManager = New BackupScheduleManager()
        AddHandler scheduleManager.BackupCompleted, AddressOf OnScheduledBackupCompleted
        AddHandler scheduleManager.BackupFailed, AddressOf OnScheduledBackupFailed
        settingsFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "pvncScheduler_settings.ini")

        ' Use the exe's embedded icon (LDSicon.ico) for the window and tray
        Me.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath)

        ' Tray icon is always shown so the app can be reopened after the window is closed
        notifyIcon.Icon = Me.Icon
        notifyIcon.Visible = True

        ' When launched with --minimized (e.g. from a Startup shortcut), start straight to the tray
        startHidden = Environment.GetCommandLineArgs().Any(Function(a) a.Equals(MinimizedArgument, StringComparison.OrdinalIgnoreCase))
        If startHidden Then
            Me.Opacity = 0
            Me.ShowInTaskbar = False
        End If
    End Sub

    Private Sub Form1_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        If startHidden Then
            Me.Hide()
            Me.Opacity = 1
            Me.ShowInTaskbar = True
        End If
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadStartWithWindows()

        ' Load saved settings from INI file
        LoadSettings()

        ' Scan the saved backup folder (must run after LoadSettings sets the folder)
        LoadBackupsList()

        ' Start scheduler if enabled
        If chkEnableSchedule.Checked And cmbSchedule.Text <> "Disabled" Then
            StartScheduler()
        End If
    End Sub

    Private Function StartupCommand() As String
        Return String.Format("""{0}"" {1}", Application.ExecutablePath, MinimizedArgument)
    End Function

    Private Sub LoadStartWithWindows()
        ' The checkbox reflects the Windows Run entry for the current user
        Try
            Using key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(StartupRegistryKey, False)
                Dim value = If(key IsNot Nothing, TryCast(key.GetValue(StartupValueName), String), Nothing)
                loadingStartupSetting = True
                chkStartWithWindows.Checked = Not String.IsNullOrEmpty(value)
                loadingStartupSetting = False

                ' Keep the entry pointing at this exe if the program was moved or rebuilt elsewhere
                If chkStartWithWindows.Checked AndAlso value <> StartupCommand() Then
                    SetStartWithWindows(True)
                End If
            End Using
        Catch ex As Exception
            loadingStartupSetting = False
            Debug.WriteLine("Error reading startup setting: " & ex.Message)
        End Try
    End Sub

    Private Sub SetStartWithWindows(enable As Boolean)
        Using key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(StartupRegistryKey, True)
            If key Is Nothing Then
                Return
            End If
            If enable Then
                key.SetValue(StartupValueName, StartupCommand())
            Else
                key.DeleteValue(StartupValueName, False)
            End If
        End Using
    End Sub

    Private Sub chkStartWithWindows_CheckedChanged(sender As Object, e As EventArgs) Handles chkStartWithWindows.CheckedChanged
        If loadingStartupSetting Then
            Return
        End If

        Try
            SetStartWithWindows(chkStartWithWindows.Checked)
            UpdateStatusLabel(If(chkStartWithWindows.Checked, "Will start with Windows (in the tray).", "Will not start with Windows."))
        Catch ex As Exception
            UpdateStatusLabel("Could not change Start with Windows: " & ex.Message, "error")
        End Try
    End Sub

    Private Sub LoadSettings()
        loadingSettings = True
        Try
            If File.Exists(settingsFile) Then
                Dim lines = File.ReadAllLines(settingsFile)
                For Each line In lines
                    If line.StartsWith("BackupFolder=") Then
                        Dim value = line.Substring("BackupFolder=".Length)
                        ' Keep the folder even if it's missing right now (network share or USB drive not
                        ' connected yet at sign-in); scheduled backups retry until it's available
                        If Not String.IsNullOrEmpty(value) Then
                            txtBackupFolder.Text = value
                            If Not Directory.Exists(value) Then
                                UpdateStatusLabel("Backup folder not found: " & value, "warning")
                            End If
                        End If
                    ElseIf line.StartsWith("Schedule=") Then
                        Dim value = line.Substring("Schedule=".Length)
                        If cmbSchedule.Items.Contains(value) Then
                            cmbSchedule.Text = value
                            PopulateDayList()
                        End If
                    ElseIf line.StartsWith("BackupTime=") Then
                        Dim value = line.Substring("BackupTime=".Length)
                        If cmbTime.Items.Contains(value) Then
                            cmbTime.Text = value
                        End If
                    ElseIf line.StartsWith("BackupDay=") Then
                        Dim value = line.Substring("BackupDay=".Length)
                        If cmbDay.Items.Contains(value) Then
                            cmbDay.SelectedItem = value
                        End If
                    ElseIf line.StartsWith("ScheduleEnabled=") Then
                        Dim value = line.Substring("ScheduleEnabled=".Length)
                        chkEnableSchedule.Checked = Boolean.Parse(value)
                    ElseIf line.StartsWith("Database=") Then
                        Dim value = line.Substring("Database=".Length)
                        If cmbDatabase.Items.Contains(value) Then
                            cmbDatabase.SelectedItem = value
                        End If
                    End If
                Next
            End If
        Catch ex As Exception
            Debug.WriteLine("Error loading settings: " & ex.Message)
        Finally
            loadingSettings = False
        End Try
    End Sub

    Private Sub SaveSettings()
        ' Controls fire change events while settings load; saving then would overwrite
        ' settings that haven't been read yet
        If loadingSettings Then
            Return
        End If

        Try
            Dim lines As New List(Of String)
            lines.Add("Database=" & cmbDatabase.Text)
            lines.Add("BackupFolder=" & txtBackupFolder.Text)
            lines.Add("Schedule=" & cmbSchedule.Text)
            lines.Add("BackupTime=" & cmbTime.Text)
            lines.Add("BackupDay=" & SelectedBackupDay())
            lines.Add("ScheduleEnabled=" & chkEnableSchedule.Checked.ToString())
            File.WriteAllLines(settingsFile, lines)
        Catch ex As Exception
            Debug.WriteLine("Error saving settings: " & ex.Message)
        End Try
    End Sub

    Private Sub btnBrowseFolder_Click(sender As Object, e As EventArgs)
        Using folderDialog As New System.Windows.Forms.FolderBrowserDialog
            folderDialog.Description = "Select backup folder"
            If folderDialog.ShowDialog() = DialogResult.OK Then
                txtBackupFolder.Text = folderDialog.SelectedPath
                SaveSettings()
                LoadBackupsList()
            End If
        End Using
    End Sub

    Private Async Sub btnBackupNow_Click(sender As Object, e As EventArgs)
        ' The tray menu calls this directly, so check here rather than relying on the button being disabled
        If Not btnBackupNow.Enabled OrElse PostgreSQLBackupManager.IsBackupRunning Then
            UpdateStatusLabel("A backup is already running.", "warning")
            Return
        End If

        If String.IsNullOrEmpty(txtBackupFolder.Text) Then
            MessageBox.Show("Please select a backup folder first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        If Not Directory.Exists(txtBackupFolder.Text) Then
            MessageBox.Show("Backup folder does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        btnBackupNow.Enabled = False
        Dim databases = PostgreSQLBackupManager.DatabasesFor(cmbDatabase.Text)
        Dim backupFolder As String = txtBackupFolder.Text

        Try
            Dim failed As New List(Of String)
            Dim lastFile As String = Nothing
            For Each database In databases
                UpdateStatusLabel("Backing up database: " & database & "...")
                ' Run pg_dump off the UI thread so the window stays responsive
                Dim backupFile As String = Await Task.Run(Function() backupManager.CreateBackup(database, backupFolder))
                If String.IsNullOrEmpty(backupFile) Then
                    failed.Add(database)
                Else
                    lastFile = backupFile
                End If
            Next
            LoadBackupsList()

            If failed.Count = 0 Then
                If databases.Length = 1 Then
                    UpdateStatusLabel("Backup completed successfully: " & Path.GetFileName(lastFile))
                Else
                    UpdateStatusLabel("Backed up all " & databases.Length & " databases successfully.")
                End If
            Else
                Dim failedList = String.Join(", ", failed)
                UpdateStatusLabel("Backup failed for: " & failedList & ". Check PostgreSQL installation and connection.", "error")
                MessageBox.Show("Backup failed for: " & failedList & vbCrLf & vbCrLf & "Check that PostgreSQL is running and pg_dump is installed (Diagnostics can test both).", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        Catch ex As Exception
            UpdateStatusLabel("Error: " & ex.Message, "error")
            MessageBox.Show("Error during backup: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            btnBackupNow.Enabled = True
        End Try
    End Sub

    Private Sub cmbDatabase_SelectedIndexChanged(sender As Object, e As EventArgs)
        If loadingSettings Then
            Return
        End If
        SaveSettings()
        LoadBackupsList()
        ' The schedule backs up whichever database is selected
        ApplyScheduleChange()
    End Sub

    Private Sub cmbSchedule_SelectedIndexChanged(sender As Object, e As EventArgs)
        PopulateDayList()
        UpdateScheduleControls()
        ApplyScheduleChange()
    End Sub

    Private Sub cmbTime_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbTime.SelectedIndexChanged
        ApplyScheduleChange()
    End Sub

    Private Sub cmbDay_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbDay.SelectedIndexChanged
        ApplyScheduleChange()
    End Sub

    Private Sub PopulateDayList()
        ' Weekly picks a weekday; Monthly picks a day of the month (1-28 exist in every month, plus "Last day")
        suppressScheduleChange = True
        Try
            cmbDay.Items.Clear()
            Select Case cmbSchedule.Text
                Case "Weekly"
                    For Each d As DayOfWeek In [Enum].GetValues(GetType(DayOfWeek))
                        cmbDay.Items.Add(d.ToString())
                    Next
                Case "Monthly"
                    For i = 1 To 28
                        cmbDay.Items.Add(i.ToString())
                    Next
                    cmbDay.Items.Add(LastDayOfMonth)
            End Select
            If cmbDay.Items.Count > 0 Then
                cmbDay.SelectedIndex = 0
            End If
        Finally
            suppressScheduleChange = False
        End Try
    End Sub

    Private Sub UpdateScheduleControls()
        Dim active = chkEnableSchedule.Checked And cmbSchedule.Text <> "Disabled"
        cmbTime.Enabled = active
        cmbDay.Enabled = active AndAlso cmbDay.Items.Count > 0
    End Sub

    Private Function SelectedBackupDay() As String
        Return If(cmbDay.SelectedItem IsNot Nothing, cmbDay.SelectedItem.ToString(), "")
    End Function

    Private Sub ApplyScheduleChange()
        ' Restart the running scheduler so a new schedule, day or time takes effect immediately
        If suppressScheduleChange OrElse Not chkEnableSchedule.Checked Then
            Return
        End If

        If cmbSchedule.Text <> "Disabled" Then
            StartScheduler()
        Else
            SaveSettings()
            scheduleManager.StopScheduler()
            UpdateScheduleStatusLabel("Scheduled Backup: Not Enabled")
        End If
    End Sub

    Private Sub chkEnableSchedule_CheckedChanged(sender As Object, e As EventArgs)
        SaveSettings()

        UpdateScheduleControls()

        If chkEnableSchedule.Checked And cmbSchedule.Text <> "Disabled" Then
            StartScheduler()
        Else
            scheduleManager.StopScheduler()
            UpdateScheduleStatusLabel("Scheduled Backup: Not Enabled")
        End If
    End Sub

    Private Sub StartScheduler()
        If String.IsNullOrEmpty(txtBackupFolder.Text) Then
            MessageBox.Show("Please select a backup folder before enabling scheduling.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            chkEnableSchedule.Checked = False
            Return
        End If

        SaveSettings()

        scheduleManager.StartScheduler(cmbDatabase.Text, cmbSchedule.Text, SelectedBackupDay(), cmbTime.Text, txtBackupFolder.Text)
        UpdateScheduleStatusLabel("Scheduled Backup: " & cmbDatabase.Text & ", " & DescribeSchedule() & " at " & cmbTime.Text)
    End Sub

    Private Function DescribeSchedule() As String
        Select Case cmbSchedule.Text
            Case "Weekly"
                Return "Weekly on " & SelectedBackupDay() & "s"
            Case "Monthly"
                If SelectedBackupDay() = LastDayOfMonth Then
                    Return "Monthly on the last day"
                End If
                Return "Monthly on day " & SelectedBackupDay()
            Case Else
                Return cmbSchedule.Text
        End Select
    End Function

    Private Sub OnScheduledBackupCompleted()
        Me.Invoke(Sub()
                      LoadBackupsList()
                      UpdateStatusLabel("Scheduled backup completed successfully.")
                  End Sub)
    End Sub

    Private Sub OnScheduledBackupFailed(failedDatabases As String, retryAt As DateTime)
        Me.Invoke(Sub()
                      LoadBackupsList()
                      UpdateStatusLabel("Scheduled backup failed for: " & failedDatabases & ". Retrying " & retryAt.ToString("ddd HH:mm") & ".", "error")
                  End Sub)
    End Sub

    Private Sub LoadBackupsList()
        lstBackups.Items.Clear()

        If String.IsNullOrEmpty(txtBackupFolder.Text) Or Not Directory.Exists(txtBackupFolder.Text) Then
            Return
        End If

        Try
            PostgreSQLBackupManager.MoveRootBackupsIntoFolders(txtBackupFolder.Text)

            ' Show the selected database's backups, or every database's for "All databases", newest first
            Dim backupFiles As New List(Of String)
            For Each database In PostgreSQLBackupManager.DatabasesFor(cmbDatabase.Text)
                ' Remove anything beyond the newest backups so the folder matches what is listed
                backupManager.CleanupOldBackups(txtBackupFolder.Text, database)
                backupFiles.AddRange(PostgreSQLBackupManager.GetBackupFiles(txtBackupFolder.Text, database).Take(PostgreSQLBackupManager.MaxBackupsToKeep))
            Next

            ' Names end in _yyyyMMdd_HHmmss.backup, so sorting by that tail orders by backup time across databases
            Dim newestFirst = backupFiles.OrderByDescending(Function(f) Path.GetFileName(f).Substring(Path.GetFileName(f).Length - "yyyyMMdd_HHmmss.backup".Length), StringComparer.Ordinal)
            For Each file In newestFirst
                Dim fileInfo = New FileInfo(file)
                Dim displayText = String.Format("{0} ({1} MB) - {2:yyyy-MM-dd HH:mm:ss}",
                    fileInfo.Name,
                    Math.Round(fileInfo.Length / (1024.0 * 1024.0), 2),
                    fileInfo.CreationTime)
                lstBackups.Items.Add(displayText)
            Next
        Catch ex As Exception
            UpdateStatusLabel("Error loading backups: " & ex.Message, "error")
        End Try
    End Sub

    Private Sub UpdateStatusLabel(message As String, Optional status As String = "normal")
        If status = "error" Then
            lblStatus.ForeColor = System.Drawing.Color.Red
        ElseIf status = "warning" Then
            lblStatus.ForeColor = System.Drawing.Color.Orange
        Else
            lblStatus.ForeColor = System.Drawing.Color.Green
        End If
        lblStatus.Text = "Status: " & message
    End Sub

    Private Sub UpdateScheduleStatusLabel(message As String)
        lblScheduleStatus.Text = message
    End Sub

    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        ' Closing the window only hides it to the tray; use tray menu "Exit" to quit
        If Not isExiting AndAlso e.CloseReason = CloseReason.UserClosing Then
            e.Cancel = True
            Me.Hide()
            Return
        End If

        notifyIcon.Visible = False
        scheduleManager.StopScheduler()
    End Sub

    Private Sub btnDiagnostics_Click(sender As Object, e As EventArgs)
        Dim diagForm = New DiagnosticsForm()
        diagForm.ShowDialog(Me)
    End Sub

    Private Sub Form1_Resize(sender As Object, e As EventArgs)
        If Me.WindowState = FormWindowState.Minimized Then
            Me.Hide()
        End If
    End Sub

    Private Sub NotifyIcon_MouseClick(sender As Object, e As System.Windows.Forms.MouseEventArgs)
        ' Show/hide window on single click
        If e.Button = MouseButtons.Left Then
            If Me.Visible = False Then
                Me.Show()
                Me.WindowState = FormWindowState.Normal
                Me.Activate()
            Else
                Me.Hide()
                Me.WindowState = FormWindowState.Minimized
            End If
        End If
    End Sub

    Private Sub ShowWindow(sender As Object, e As EventArgs)
        RestoreWindow()
    End Sub

    Friend Sub RestoreWindow()
        LoadBackupsList()
        Me.Show()
        Me.WindowState = FormWindowState.Normal
        Me.Activate()
    End Sub

    Private Sub BackupNowFromTray(sender As Object, e As EventArgs)
        btnBackupNow_Click(Nothing, Nothing)
    End Sub

    Private Sub ExitApplication(sender As Object, e As EventArgs)
        isExiting = True
        notifyIcon.Visible = False
        scheduleManager.StopScheduler()
        Application.Exit()
    End Sub
End Class
