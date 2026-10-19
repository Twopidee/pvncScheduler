Imports System.Timers
Imports System.IO

Public Class BackupScheduleManager
    Private scheduleTimer As Timer
    Private isRunning As Boolean = False
    Private currentDatabase As String
    Private currentBackupFolder As String
    Private backupManager As PostgreSQLBackupManager
    Private lastBackupDate As DateTime = DateTime.MinValue
    Private _backupTime As String = "02:00"
    Private currentSchedule As String = "Daily"
    ' Weekday name for Weekly ("Sunday"...), day number or "Last day" for Monthly
    Private currentBackupDay As String = ""

    Public Event BackupCompleted()

    Public Property BackupTime As String
        Get
            Return _backupTime
        End Get
        Set(value As String)
            _backupTime = value
        End Set
    End Property

    Public Sub StartScheduler(database As String, schedule As String, backupDay As String, backupTime As String, backupFolder As String)
        Try
            If isRunning Then
                StopScheduler()
            End If

            Me.BackupTime = backupTime
            currentSchedule = schedule
            currentBackupDay = backupDay
            currentDatabase = database
            currentBackupFolder = backupFolder
            backupManager = New PostgreSQLBackupManager()

            scheduleTimer = New Timer()
            scheduleTimer.AutoReset = True
            AddHandler scheduleTimer.Elapsed, AddressOf TimerTick

            ' Set interval to check every minute (60000 ms)
            scheduleTimer.Interval = 60000

            scheduleTimer.Start()
            isRunning = True
            Debug.WriteLine("Scheduler started for " & database)
        Catch ex As Exception
            Debug.WriteLine("Error starting scheduler: " & ex.Message)
        End Try
    End Sub

    Public Sub StopScheduler()
        Try
            If scheduleTimer IsNot Nothing Then
                scheduleTimer.Stop()
                scheduleTimer.Dispose()
                scheduleTimer = Nothing
            End If
            isRunning = False
            Debug.WriteLine("Scheduler stopped")
        Catch ex As Exception
            Debug.WriteLine("Error stopping scheduler: " & ex.Message)
        End Try
    End Sub

    Private Sub TimerTick(sender As Object, e As ElapsedEventArgs)
        Try
            Dim currentTime = DateTime.Now
            Dim backupTimeSpan = TimeSpan.Parse(Me.BackupTime)
            Dim targetTime = currentTime.Date.Add(backupTimeSpan)

            ' Check if we should run backup (scheduled day, within 1 minute window and not already run today)
            If IsScheduledDay(currentTime) AndAlso currentTime >= targetTime AndAlso currentTime < targetTime.AddMinutes(1) AndAlso lastBackupDate.Date < DateTime.Now.Date Then
                PerformBackup()
                lastBackupDate = DateTime.Now
            End If
        Catch ex As Exception
            Debug.WriteLine("Timer error: " & ex.Message)
        End Try
    End Sub

    Private Function IsScheduledDay(day As DateTime) As Boolean
        Select Case currentSchedule
            Case "Daily"
                Return True
            Case "Weekly"
                Return day.DayOfWeek.ToString() = currentBackupDay
            Case "Monthly"
                If currentBackupDay = Form1.LastDayOfMonth Then
                    Return day.Day = DateTime.DaysInMonth(day.Year, day.Month)
                End If
                Dim dayOfMonth As Integer
                Return Integer.TryParse(currentBackupDay, dayOfMonth) AndAlso day.Day = dayOfMonth
            Case Else
                Return False
        End Select
    End Function

    Private Sub PerformBackup()
        Try
            Debug.WriteLine("Executing scheduled backup for " & currentDatabase)
            Dim backupFile = backupManager.CreateBackup(currentDatabase, currentBackupFolder)

            If Not String.IsNullOrEmpty(backupFile) Then
                Debug.WriteLine("Scheduled backup completed: " & backupFile)
                RaiseEvent BackupCompleted()
            Else
                Debug.WriteLine("Scheduled backup failed")
            End If
        Catch ex As Exception
            Debug.WriteLine("Backup execution error: " & ex.Message)
        End Try
    End Sub
End Class
