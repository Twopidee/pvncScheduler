Imports System.Diagnostics
Imports System.IO

Public Class PostgreSQLBackupManager
    Private pgDumpExecutable As String
    Public Const MaxBackupsToKeep As Integer = 10
    ' Shared across all instances: 1 while a pg_dump is running
    Private Shared backupInProgress As Integer = 0

    ' Databases this program backs up. Each saves to a subfolder of the backup folder named after the database.
    Public Shared ReadOnly Databases As String() = {"pvnc", "bizinfo", "bizmaint", "postgres"}
    Public Const AllDatabases As String = "All databases"

    ' The databases a dropdown selection stands for: one, or all of them for "All databases"
    Public Shared Function DatabasesFor(selection As String) As String()
        If selection = AllDatabases Then
            Return Databases
        End If
        Return {selection}
    End Function

    Public Shared Function BackupFolderFor(rootFolder As String, database As String) As String
        Return Path.Combine(rootFolder, database)
    End Function

    Private Shared Function BackupNamePattern(database As String) As String
        Return "^" & System.Text.RegularExpressions.Regex.Escape(database) & "_\d{8}_\d{6}\.backup$"
    End Function

    ' Older versions saved pvnc backups directly in the backup folder. Move any backups this program made
    ' that are still there into their database's subfolder, so they're listed and kept by retention.
    Public Shared Sub MoveRootBackupsIntoFolders(rootFolder As String)
        If Not Directory.Exists(rootFolder) Then
            Return
        End If

        For Each database In Databases
            Dim namePattern = BackupNamePattern(database)
            For Each file In Directory.GetFiles(rootFolder, database & "_*.backup")
                Dim fileName = Path.GetFileName(file)
                If Not System.Text.RegularExpressions.Regex.IsMatch(fileName, namePattern) Then
                    Continue For
                End If
                Try
                    Dim targetFolder = BackupFolderFor(rootFolder, database)
                    Dim target = Path.Combine(targetFolder, fileName)
                    If Not IO.File.Exists(target) Then
                        Directory.CreateDirectory(targetFolder)
                        IO.File.Move(file, target)
                        Debug.WriteLine("Moved " & fileName & " into " & targetFolder)
                    End If
                Catch ex As Exception
                    Debug.WriteLine("Could not move " & fileName & ": " & ex.Message)
                End Try
            Next
        Next
    End Sub

    Public Sub New()
        pgDumpExecutable = FindPgDump()
    End Sub

    Private Function FindPgDump() As String
        ' Try common PostgreSQL installation paths
        Dim commonPaths As String() = {
            "C:\BouMatic\postgresql\bin\pg_dump.exe",
            "C:\Program Files\PostgreSQL\15\bin\pg_dump.exe",
            "C:\Program Files\PostgreSQL\14\bin\pg_dump.exe",
            "C:\Program Files\PostgreSQL\13\bin\pg_dump.exe",
            "C:\Program Files\PostgreSQL\12\bin\pg_dump.exe",
            "C:\Program Files\PostgreSQL\11\bin\pg_dump.exe",
            "C:\Program Files\PostgreSQL\10\bin\pg_dump.exe",
            "C:\Program Files\PostgreSQL\9.6\bin\pg_dump.exe",
            "C:\Program Files\PostgreSQL\9.2\bin\pg_dump.exe",
            "C:\Program Files (x86)\PostgreSQL\15\bin\pg_dump.exe",
            "C:\Program Files (x86)\PostgreSQL\14\bin\pg_dump.exe",
            "C:\Program Files (x86)\PostgreSQL\13\bin\pg_dump.exe",
            "C:\Program Files (x86)\PostgreSQL\12\bin\pg_dump.exe",
            "C:\Program Files (x86)\PostgreSQL\11\bin\pg_dump.exe",
            "C:\Program Files (x86)\PostgreSQL\10\bin\pg_dump.exe",
            "C:\Program Files (x86)\PostgreSQL\9.6\bin\pg_dump.exe",
            "C:\Program Files (x86)\PostgreSQL\9.2\bin\pg_dump.exe",
            "C:\BouMatic\postgresql\bin\"
        }

        ' Check common paths first
        For Each path In commonPaths
            If File.Exists(path) Then
                Debug.WriteLine("Found pg_dump at: " & path)
                Return path
            End If
        Next

        ' Fall back to PATH lookup
        Debug.WriteLine("pg_dump not found in common paths, trying PATH")
        Return "pg_dump"
    End Function

    Public Shared ReadOnly Property IsBackupRunning As Boolean
        Get
            Return System.Threading.Volatile.Read(backupInProgress) <> 0
        End Get
    End Property

    Public Function CreateBackup(database As String, rootFolder As String) As String
        ' Only one pg_dump at a time, whether started by the button, the tray menu or the scheduler
        If System.Threading.Interlocked.CompareExchange(backupInProgress, 1, 0) <> 0 Then
            Debug.WriteLine("Backup skipped: another backup is already running")
            Return Nothing
        End If

        Try
            Return RunBackup(database, rootFolder)
        Finally
            System.Threading.Interlocked.Exchange(backupInProgress, 0)
        End Try
    End Function

    Private Function RunBackup(database As String, rootFolder As String) As String
        Try
            Dim backupFolder = BackupFolderFor(rootFolder, database)

            ' Create backup folder if it doesn't exist
            If Not Directory.Exists(backupFolder) Then
                Directory.CreateDirectory(backupFolder)
            End If

            ' Verify pg_dump is available
            If String.IsNullOrEmpty(pgDumpExecutable) OrElse (pgDumpExecutable = "pg_dump" AndAlso Not IsPgDumpInPath()) Then
                Debug.WriteLine("pg_dump not found")
                Return Nothing
            End If

            ' Generate backup filename with timestamp
            Dim timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss")
            Dim backupFileName = String.Format("{0}_{1}.backup", database, timestamp)
            Dim backupFilePath = Path.Combine(backupFolder, backupFileName)

            ' Setup process to run pg_dump with proper binary archive output
            Dim processInfo = New ProcessStartInfo
            processInfo.FileName = pgDumpExecutable

            ' pg_dump arguments:
            ' -F c specifies custom format (binary archive, same as pgAdmin .backup)
            ' -h localhost connects to local PostgreSQL
            ' -U postgres uses postgres user
            ' -w never prompts for a password (fails instead of hanging if none is available)
            ' -f writes the archive directly to the file (piping custom format through
            '    stdout makes older pg_dump emit "ftell mismatch" warnings and a bloated archive)
            ' Database name to backup

            processInfo.Arguments = String.Format("-F c -h localhost -U postgres -w -f ""{0}"" ""{1}""", backupFilePath, database)
            processInfo.UseShellExecute = False
            processInfo.RedirectStandardOutput = True
            processInfo.RedirectStandardError = True
            processInfo.RedirectStandardInput = True
            processInfo.CreateNoWindow = True

            ' No password is passed: the local server trusts connections from this PC. If one is ever
            ' required, pg_dump reads it from %APPDATA%\postgresql\pgpass.conf.

            Debug.WriteLine("Starting pg_dump with: " & processInfo.FileName & " " & processInfo.Arguments)

            Dim process As Process = Process.Start(processInfo)
            If process IsNot Nothing Then
                ' Close stdin to prevent any input blocking
                process.StandardInput.Close()

                ' Drain stdout/stderr asynchronously so pg_dump never blocks on a full pipe
                Dim outputTask = process.StandardOutput.ReadToEndAsync()
                Dim errorTask = process.StandardError.ReadToEndAsync()

                ' Now wait for process to complete with timeout
                Dim completed = process.WaitForExit(600000) ' 10 minute timeout

                If Not completed Then
                    Debug.WriteLine("pg_dump timed out")
                    Try
                        process.Kill()
                    Catch
                    End Try
                    If File.Exists(backupFilePath) Then
                        File.Delete(backupFilePath)
                    End If
                    process.Dispose()
                    Return Nothing
                End If

                ' Get error output
                Dim errorOutput = ""
                Try
                    errorOutput = errorTask.Result
                Catch
                End Try

                ' Check results
                Dim fileInfo = New FileInfo(backupFilePath)
                If process.ExitCode = 0 AndAlso fileInfo.Exists AndAlso fileInfo.Length > 0 Then
                    Debug.WriteLine("Backup succeeded: " & backupFilePath & " (" & fileInfo.Length & " bytes, binary archive)")
                    ' Cleanup old backups, keep only 10 most recent
                    CleanupOldBackups(rootFolder, database)
                    process.Dispose()
                    Return backupFilePath
                Else
                    Debug.WriteLine("Backup failed with exit code: " & process.ExitCode)
                    Debug.WriteLine("Error output: " & errorOutput)
                    Debug.WriteLine("File size: " & If(fileInfo.Exists, fileInfo.Length, 0))
                    ' If pg_dump fails, remove the file
                    If File.Exists(backupFilePath) Then
                        File.Delete(backupFilePath)
                    End If
                    process.Dispose()
                    Return Nothing
                End If
            Else
                Return Nothing
            End If
        Catch ex As Exception
            Debug.WriteLine("Backup error: " & ex.Message & vbCrLf & ex.StackTrace)
            Return Nothing
        End Try
    End Function

    Private Function IsPgDumpInPath() As Boolean
        Try
            Dim pinfo = New ProcessStartInfo
            pinfo.FileName = "cmd.exe"
            pinfo.Arguments = "/c where pg_dump"
            pinfo.UseShellExecute = False
            pinfo.RedirectStandardOutput = True
            pinfo.CreateNoWindow = True

            Using proc = Process.Start(pinfo)
                Dim output = proc.StandardOutput.ReadToEnd()
                proc.WaitForExit()
                Return proc.ExitCode = 0 And Not String.IsNullOrWhiteSpace(output)
            End Using
        Catch
            Return False
        End Try
    End Function

    Public Shared Function GetBackupFiles(rootFolder As String, database As String) As List(Of String)
        ' Only backups this program created for this database (<database>_yyyyMMdd_HHmmss.backup), newest
        ' first. The timestamp in the name sorts correctly and, unlike file dates, isn't changed by OneDrive
        ' or copying. Both the on-screen list and the cleanup use this, so whatever is listed is exactly what is kept.
        Dim backupFolder = BackupFolderFor(rootFolder, database)
        If Not Directory.Exists(backupFolder) Then
            Return New List(Of String)
        End If

        Dim namePattern = BackupNamePattern(database)
        Return Directory.GetFiles(backupFolder, database & "_*.backup").
            Where(Function(f) System.Text.RegularExpressions.Regex.IsMatch(Path.GetFileName(f), namePattern)).
            OrderByDescending(Function(f) Path.GetFileName(f), StringComparer.Ordinal).
            ToList()
    End Function

    Public Sub CleanupOldBackups(rootFolder As String, database As String)
        Try
            Dim backupFiles = GetBackupFiles(rootFolder, database)

            ' Keep only the most recent backups, deleting the oldest
            If backupFiles.Count > MaxBackupsToKeep Then
                For i = MaxBackupsToKeep To backupFiles.Count - 1
                    Try
                        File.Delete(backupFiles(i))
                    Catch ex As Exception
                        Debug.WriteLine("Error deleting old backup: " & ex.Message)
                    End Try
                Next
            End If
        Catch ex As Exception
            Debug.WriteLine("Cleanup error: " & ex.Message)
        End Try
    End Sub
End Class
