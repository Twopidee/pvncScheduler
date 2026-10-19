Imports System.Diagnostics

Public Class DiagnosticsForm
    Inherits System.Windows.Forms.Form

    Private Sub DiagnosticsForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "PostgreSQL Diagnostics"
        Me.Size = New System.Drawing.Size(600, 500)
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent

        ' Create text box for output
        Dim txtOutput = New System.Windows.Forms.TextBox()
        txtOutput.Multiline = True
        txtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both
        txtOutput.ReadOnly = True
        txtOutput.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Controls.Add(txtOutput)

        ' Create panel for buttons
        Dim pnlButtons = New System.Windows.Forms.Panel()
        pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom
        pnlButtons.Height = 50
        Me.Controls.Add(pnlButtons)

        ' Create buttons
        Dim btnCheckPgDump = New System.Windows.Forms.Button()
        btnCheckPgDump.Text = "Check pg_dump"
        btnCheckPgDump.Location = New System.Drawing.Point(10, 10)
        btnCheckPgDump.Size = New System.Drawing.Size(120, 30)
        AddHandler btnCheckPgDump.Click, Sub()
                                             txtOutput.Text = "Checking pg_dump installation..." & vbCrLf & vbCrLf
                                             txtOutput.AppendText(DiagnosticsHelper.CheckPgDumpInstallation())
                                         End Sub
        pnlButtons.Controls.Add(btnCheckPgDump)

        Dim btnCheckConnection = New System.Windows.Forms.Button()
        btnCheckConnection.Text = "Test Connection"
        btnCheckConnection.Location = New System.Drawing.Point(140, 10)
        btnCheckConnection.Size = New System.Drawing.Size(120, 30)
        AddHandler btnCheckConnection.Click, Sub()
                                                 txtOutput.Text = "Testing PostgreSQL connection..." & vbCrLf & vbCrLf
                                                 txtOutput.AppendText(DiagnosticsHelper.TestPostgreSQLConnection("localhost", "postgres"))
                                             End Sub
        pnlButtons.Controls.Add(btnCheckConnection)

        Dim btnClose = New System.Windows.Forms.Button()
        btnClose.Text = "Close"
        btnClose.Location = New System.Drawing.Point(270, 10)
        btnClose.Size = New System.Drawing.Size(100, 30)
        AddHandler btnClose.Click, Sub() Me.Close()
        pnlButtons.Controls.Add(btnClose)

        ' Run initial diagnostics
        txtOutput.Text = "=== pvnc Backup Manager Diagnostics ===" & vbCrLf & vbCrLf
        txtOutput.AppendText("Checking pg_dump installation..." & vbCrLf)
        txtOutput.AppendText(DiagnosticsHelper.CheckPgDumpInstallation() & vbCrLf & vbCrLf)
    End Sub
End Class
