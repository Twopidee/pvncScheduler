Imports System.Diagnostics
Imports System.IO

Public Class PostgreSQLBackupManager
    Private pgDumpExecutable As String
    Public Const MaxBackupsToKeep As Integer = 10

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

    Public Function CreateBackup(database As String, backupFolder As String) As String
        Try
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
            Dim backupFileName = String.Format("pvnc_{0}.backup", timestamp)
            Dim backupFilePath = Path.Combine(backupFolder, backupFileName)

            ' Setup process to run pg_dump with proper binary archive output
            Dim processInfo = New ProcessStartInfo
            processInfo.FileName = pgDumpExecutable

            ' pg_dump arguments:
            ' -F c specifies custom format (binary archive, same as pgAdmin .backup)
            ' -h localhost connects to local PostgreSQL
            ' -U postgres uses postgres user
            ' -f writes the archive directly to the file (piping custom format through
            '    stdout makes older pg_dump emit "ftell mismatch" warnings and a bloated archive)
            ' Database name to backup

            processInfo.Arguments = String.Format("-F c -h localhost -U postgres -f ""{0}"" ""{1}""", backupFilePath, database)
            processInfo.UseShellExecute = False
            processInfo.RedirectStandardOutput = True
            processInfo.RedirectStandardError = True
            processInfo.RedirectStandardInput = True
            processInfo.CreateNoWindow = True

            ' SET PASSWORD VIA ENVIRONMENT VARIABLE - PostgreSQL will use this and won't prompt
            processInfo.EnvironmentVariables("PGPASSWORD") = "bou2004"

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
                    CleanupOldBackups(backupFolder)
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

    Public Sub CleanupOldBackups(backupFolder As String)
        Try
            If Not Directory.Exists(backupFolder) Then
                Return
            End If

            ' Only touch backups this program created (pvnc_yyyyMMdd_HHmmss.backup). The timestamp in
            ' the name sorts newest-first and, unlike file dates, isn't changed by OneDrive or copying.
            Dim backupFiles = Directory.GetFiles(backupFolder, "pvnc_*.backup").
                Where(Function(f) System.Text.RegularExpressions.Regex.IsMatch(Path.GetFileName(f), "^pvnc_\d{8}_\d{6}\.backup$")).
                OrderByDescending(Function(f) Path.GetFileName(f), StringComparer.Ordinal).
                ToList()

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
