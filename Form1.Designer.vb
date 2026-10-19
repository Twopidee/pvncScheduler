<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New System.ComponentModel.Container()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.txtBackupFolder = New System.Windows.Forms.TextBox()
        Me.btnBrowseFolder = New System.Windows.Forms.Button()
        Me.btnBackupNow = New System.Windows.Forms.Button()
        Me.cmbDatabase = New System.Windows.Forms.ComboBox()
        Me.cmbSchedule = New System.Windows.Forms.ComboBox()
        Me.cmbTime = New System.Windows.Forms.ComboBox()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.cmbDay = New System.Windows.Forms.ComboBox()
        Me.chkEnableSchedule = New System.Windows.Forms.CheckBox()
        Me.chkStartWithWindows = New System.Windows.Forms.CheckBox()
        Me.lstBackups = New System.Windows.Forms.ListBox()
        Me.lblStatus = New System.Windows.Forms.Label()
        Me.lblScheduleStatus = New System.Windows.Forms.Label()
        Me.btnDiagnostics = New System.Windows.Forms.Button()
        Me.notifyIcon = New System.Windows.Forms.NotifyIcon(components)
        Me.contextMenuStrip = New System.Windows.Forms.ContextMenuStrip(components)
        Me.SuspendLayout()

        ' Label1 - Database Label
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(12, 15)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(56, 13)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Database:"

        ' Label2 - Backup Folder Label
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(12, 50)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(78, 13)
        Me.Label2.TabIndex = 1
        Me.Label2.Text = "Backup Folder:"

        ' Label3 - Schedule Label
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(12, 85)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(62, 13)
        Me.Label3.TabIndex = 2
        Me.Label3.Text = "Schedule:"

        ' Label4 - Backup Time Label
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(12, 120)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(79, 13)
        Me.Label4.TabIndex = 3
        Me.Label4.Text = "Backup Time:"

        ' Label5 - Recent Backups Label
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(12, 180)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(94, 13)
        Me.Label5.TabIndex = 4
        Me.Label5.Text = "Recent Backups:"

        ' txtBackupFolder
        Me.txtBackupFolder.Location = New System.Drawing.Point(100, 47)
        Me.txtBackupFolder.Name = "txtBackupFolder"
        Me.txtBackupFolder.ReadOnly = True
        Me.txtBackupFolder.Size = New System.Drawing.Size(400, 20)
        Me.txtBackupFolder.TabIndex = 5

        ' btnBrowseFolder
        Me.btnBrowseFolder.Location = New System.Drawing.Point(506, 47)
        Me.btnBrowseFolder.Name = "btnBrowseFolder"
        Me.btnBrowseFolder.Size = New System.Drawing.Size(75, 23)
        Me.btnBrowseFolder.TabIndex = 6
        Me.btnBrowseFolder.Text = "Browse..."
        Me.btnBrowseFolder.UseVisualStyleBackColor = True
        AddHandler Me.btnBrowseFolder.Click, AddressOf Me.btnBrowseFolder_Click

        ' btnBackupNow
        Me.btnBackupNow.Location = New System.Drawing.Point(506, 12)
        Me.btnBackupNow.Name = "btnBackupNow"
        Me.btnBackupNow.Size = New System.Drawing.Size(100, 23)
        Me.btnBackupNow.TabIndex = 7
        Me.btnBackupNow.Text = "Create Backup Now"
        Me.btnBackupNow.UseVisualStyleBackColor = True
        AddHandler Me.btnBackupNow.Click, AddressOf Me.btnBackupNow_Click

        ' btnDiagnostics
        Me.btnDiagnostics.Location = New System.Drawing.Point(612, 12)
        Me.btnDiagnostics.Name = "btnDiagnostics"
        Me.btnDiagnostics.Size = New System.Drawing.Size(75, 23)
        Me.btnDiagnostics.TabIndex = 15
        Me.btnDiagnostics.Text = "Diagnostics"
        Me.btnDiagnostics.UseVisualStyleBackColor = True
        AddHandler Me.btnDiagnostics.Click, AddressOf Me.btnDiagnostics_Click

        ' cmbDatabase
        Me.cmbDatabase.FormattingEnabled = True
        Me.cmbDatabase.Items.AddRange(New Object() {"pvnc"})
        Me.cmbDatabase.Location = New System.Drawing.Point(100, 12)
        Me.cmbDatabase.Name = "cmbDatabase"
        Me.cmbDatabase.Size = New System.Drawing.Size(350, 21)
        Me.cmbDatabase.TabIndex = 8
        Me.cmbDatabase.Text = "pvnc"

        ' cmbSchedule
        Me.cmbSchedule.FormattingEnabled = True
        Me.cmbSchedule.Items.AddRange(New Object() {"Disabled", "Daily", "Weekly", "Monthly"})
        Me.cmbSchedule.Location = New System.Drawing.Point(100, 82)
        Me.cmbSchedule.Name = "cmbSchedule"
        Me.cmbSchedule.Size = New System.Drawing.Size(200, 21)
        Me.cmbSchedule.TabIndex = 9
        Me.cmbSchedule.Text = "Disabled"
        AddHandler Me.cmbSchedule.SelectedIndexChanged, AddressOf Me.cmbSchedule_SelectedIndexChanged

        ' cmbTime
        Me.cmbTime.FormattingEnabled = True
        Me.cmbTime.Items.AddRange(New Object() {"00:00", "01:00", "02:00", "03:00", "04:00", "05:00", "06:00", "07:00", "08:00", "09:00", "10:00", "11:00", "12:00", "13:00", "14:00", "15:00", "16:00", "17:00", "18:00", "19:00", "20:00", "21:00", "22:00", "23:00"})
        Me.cmbTime.Location = New System.Drawing.Point(100, 117)
        Me.cmbTime.Name = "cmbTime"
        Me.cmbTime.Size = New System.Drawing.Size(200, 21)
        Me.cmbTime.TabIndex = 10
        Me.cmbTime.Text = "02:00"
        Me.cmbTime.Enabled = False

        ' Label6 - Backup Day Label
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(310, 120)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(29, 13)
        Me.Label6.TabIndex = 16
        Me.Label6.Text = "Day:"

        ' cmbDay
        Me.cmbDay.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbDay.FormattingEnabled = True
        Me.cmbDay.Location = New System.Drawing.Point(345, 117)
        Me.cmbDay.Name = "cmbDay"
        Me.cmbDay.Size = New System.Drawing.Size(155, 21)
        Me.cmbDay.TabIndex = 17
        Me.cmbDay.Enabled = False

        ' chkEnableSchedule
        Me.chkEnableSchedule.AutoSize = True
        Me.chkEnableSchedule.Location = New System.Drawing.Point(310, 82)
        Me.chkEnableSchedule.Name = "chkEnableSchedule"
        Me.chkEnableSchedule.Size = New System.Drawing.Size(65, 17)
        Me.chkEnableSchedule.TabIndex = 11
        Me.chkEnableSchedule.Text = "Enabled"
        Me.chkEnableSchedule.UseVisualStyleBackColor = True
        Me.chkEnableSchedule.Checked = False
        AddHandler Me.chkEnableSchedule.CheckedChanged, AddressOf Me.chkEnableSchedule_CheckedChanged

        ' chkStartWithWindows
        Me.chkStartWithWindows.AutoSize = True
        Me.chkStartWithWindows.Location = New System.Drawing.Point(100, 150)
        Me.chkStartWithWindows.Name = "chkStartWithWindows"
        Me.chkStartWithWindows.Size = New System.Drawing.Size(117, 17)
        Me.chkStartWithWindows.TabIndex = 18
        Me.chkStartWithWindows.Text = "Start with Windows"
        Me.chkStartWithWindows.UseVisualStyleBackColor = True

        ' lstBackups
        Me.lstBackups.FormattingEnabled = True
        Me.lstBackups.Location = New System.Drawing.Point(12, 196)
        Me.lstBackups.Name = "lstBackups"
        Me.lstBackups.Size = New System.Drawing.Size(675, 238)
        Me.lstBackups.TabIndex = 12

        ' lblStatus
        Me.lblStatus.AutoSize = True
        Me.lblStatus.ForeColor = System.Drawing.Color.Green
        Me.lblStatus.Location = New System.Drawing.Point(12, 437)
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Size = New System.Drawing.Size(90, 13)
        Me.lblStatus.TabIndex = 13
        Me.lblStatus.Text = "Status: Ready"

        ' lblScheduleStatus
        Me.lblScheduleStatus.AutoSize = True
        Me.lblScheduleStatus.ForeColor = System.Drawing.Color.Blue
        Me.lblScheduleStatus.Location = New System.Drawing.Point(400, 437)
        Me.lblScheduleStatus.Name = "lblScheduleStatus"
        Me.lblScheduleStatus.Size = New System.Drawing.Size(181, 13)
        Me.lblScheduleStatus.TabIndex = 14
        Me.lblScheduleStatus.Text = "Scheduled Backup: Not Enabled"

        ' notifyIcon
        Me.notifyIcon.Text = "pvnc Backup Manager"
        Me.notifyIcon.Visible = False
        AddHandler Me.notifyIcon.MouseClick, AddressOf Me.NotifyIcon_MouseClick

        ' contextMenuStrip
        Me.contextMenuStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {
            New System.Windows.Forms.ToolStripMenuItem("Show", Nothing, AddressOf Me.ShowWindow),
            New System.Windows.Forms.ToolStripMenuItem("Backup Now", Nothing, AddressOf Me.BackupNowFromTray),
            New System.Windows.Forms.ToolStripSeparator(),
            New System.Windows.Forms.ToolStripMenuItem("Exit", Nothing, AddressOf Me.ExitApplication)
        })
        Me.notifyIcon.ContextMenuStrip = Me.contextMenuStrip

        ' Form1
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(699, 460)
        Me.Controls.Add(Me.btnDiagnostics)
        Me.Controls.Add(Me.lblScheduleStatus)
        Me.Controls.Add(Me.lblStatus)
        Me.Controls.Add(Me.lstBackups)
        Me.Controls.Add(Me.chkStartWithWindows)
        Me.Controls.Add(Me.chkEnableSchedule)
        Me.Controls.Add(Me.cmbDay)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.cmbTime)
        Me.Controls.Add(Me.cmbSchedule)
        Me.Controls.Add(Me.btnBackupNow)
        Me.Controls.Add(Me.cmbDatabase)
        Me.Controls.Add(Me.btnBrowseFolder)
        Me.Controls.Add(Me.txtBackupFolder)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Name = "Form1"
        Me.Text = "pvnc Backup Manager"
        AddHandler Me.Resize, AddressOf Form1_Resize
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

    Private Label1 As System.Windows.Forms.Label
    Private Label2 As System.Windows.Forms.Label
    Private Label3 As System.Windows.Forms.Label
    Private Label4 As System.Windows.Forms.Label
    Private Label5 As System.Windows.Forms.Label
    Private WithEvents txtBackupFolder As System.Windows.Forms.TextBox
    Private WithEvents btnBrowseFolder As System.Windows.Forms.Button
    Private WithEvents btnBackupNow As System.Windows.Forms.Button
    Private WithEvents cmbDatabase As System.Windows.Forms.ComboBox
    Private WithEvents cmbSchedule As System.Windows.Forms.ComboBox
    Private WithEvents cmbTime As System.Windows.Forms.ComboBox
    Private Label6 As System.Windows.Forms.Label
    Private WithEvents cmbDay As System.Windows.Forms.ComboBox
    Private WithEvents chkEnableSchedule As System.Windows.Forms.CheckBox
    Private WithEvents chkStartWithWindows As System.Windows.Forms.CheckBox
    Private lstBackups As System.Windows.Forms.ListBox
    Private lblStatus As System.Windows.Forms.Label
    Private lblScheduleStatus As System.Windows.Forms.Label
    Private WithEvents btnDiagnostics As System.Windows.Forms.Button
    Private notifyIcon As System.Windows.Forms.NotifyIcon
    Private contextMenuStrip As System.Windows.Forms.ContextMenuStrip

End Class
