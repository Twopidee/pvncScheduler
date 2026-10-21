Imports System.Diagnostics
Imports System.IO

Public Class DiagnosticsHelper
    Public Shared Function CheckPgDumpInstallation() As String
        Try
            ' Try to find pg_dump in common locations
            Dim commonPaths As String() = {
                "C:\BouMatic\postgresql\bin\pg_dump.exe",
                "C:\Program Files\PostgreSQL\15\bin\pg_dump.exe",
                "C:\Program Files\PostgreSQL\14\bin\pg_dump.exe",
                "C:\Program Files\PostgreSQL\13\bin\pg_dump.exe",
                "C:\Program Files\PostgreSQL\12\bin\pg_dump.exe",
                "C:\Program Files\PostgreSQL\11\bin\pg_dump.exe",
                "C:\Program Files\PostgreSQL\10\bin\pg_dump.exe",
                "C:\Program Files\PostgreSQL\9.6\bin\pg_dump.exe",
                "C:\Program Files (x86)\PostgreSQL\15\bin\pg_dump.exe",
                "C:\Program Files (x86)\PostgreSQL\14\bin\pg_dump.exe",
                "C:\Program Files (x86)\PostgreSQL\13\bin\pg_dump.exe"
            }

            For Each path In commonPaths
                If File.Exists(path) Then
                    Return "Found pg_dump at: " & path
                End If
            Next

            ' Try to run pg_dump from PATH
            Dim pinfo = New ProcessStartInfo
            pinfo.FileName = "cmd.exe"
            pinfo.Arguments = "/c where pg_dump"
            pinfo.UseShellExecute = False
            pinfo.RedirectStandardOutput = True
            pinfo.CreateNoWindow = True

            Using proc = Process.Start(pinfo)
                Dim output = proc.StandardOutput.ReadToEnd()
                proc.WaitForExit()

                If proc.ExitCode = 0 And Not String.IsNullOrWhiteSpace(output) Then
                    Return "pg_dump found in PATH: " & output.Trim()
                End If
            End Using

            Return "ERROR: pg_dump not found in PATH or common installation directories." & vbCrLf &
                   "Please install PostgreSQL or add its bin folder to your system PATH."

        Catch ex As Exception
            Return "ERROR: " & ex.Message
        End Try
    End Function

    Public Shared Function TestPostgreSQLConnection(host As String, username As String, password As String) As String
        Try
            Dim pinfo = New ProcessStartInfo
            pinfo.FileName = "C:\BouMatic\postgresql\bin\psql.exe"
            ' -w never prompts for a password, so psql can't hang waiting on a hidden prompt
            pinfo.Arguments = String.Format("-h {0} -U {1} -w -c ""SELECT version();""", host, username)
            pinfo.UseShellExecute = False
            pinfo.RedirectStandardOutput = True
            pinfo.RedirectStandardError = True
            pinfo.CreateNoWindow = True
            If Not String.IsNullOrEmpty(password) Then
                pinfo.EnvironmentVariables("PGPASSWORD") = password
            End If

            Using proc = Process.Start(pinfo)
                Dim output = proc.StandardOutput.ReadToEnd()
                Dim errors = proc.StandardError.ReadToEnd()
                proc.WaitForExit()

                If proc.ExitCode = 0 Then
                    Return "Connection successful!" & vbCrLf & output
                Else
                    Return "Connection failed: " & errors
                End If
            End Using

        Catch ex As Exception
            Return "ERROR: " & ex.Message
        End Try
    End Function
End Class
