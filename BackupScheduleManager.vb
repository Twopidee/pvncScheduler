Imports System.Timers
Imports System.IO

Public Class BackupScheduleManager
    ' How long to wait before retrying a scheduled backup that failed
    Private Shared ReadOnly RetryInterval As TimeSpan = TimeSpan.FromMinutes(15)

    Private scheduleTimer As Timer
    Private isRunning As Boolean = False
    Private currentDatabase As String
    Private currentBackupFolder As String
    Private backupManager As PostgreSQLBackupManager
    Private _backupTime As String = "02:00"
    Private currentSchedule As String = "Daily"
    ' Weekday name for Weekly ("Sunday"...), day number or "Last day" for Monthly
    Private currentBackupDay As String = ""
    ' When the next backup (or retry) is due. Any tick at or after this time runs it, so a tick that
    ' lands late or a PC waking from sleep still gets its backup instead of missing a one-minute window.
    Private nextRun As DateTime = DateTime.MaxValue
    Private ReadOnly scheduleLock As New Object()
    ' 1 while a tick is running a backup, so ticks that fire during a long backup don't start another
    Private tickInProgress As Integer = 0
    ' Databases still to retry after a failure; Nothing means the next run backs up all of them
    Private pendingDatabases As List(Of String)

    Public Event BackupCompleted()
    Public Event BackupFailed(failedDatabases As String, retryAt As DateTime)

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

            SyncLock scheduleLock
                nextRun = NextOccurrence(DateTime.Now)
                pendingDatabases = Nothing
            End SyncLock

            scheduleTimer = New Timer()
            scheduleTimer.AutoReset = True
            AddHandler scheduleTimer.Elapsed, AddressOf TimerTick

            ' Set interval to check every minute (60000 ms)
            scheduleTimer.Interval = 60000

            scheduleTimer.Start()
            isRunning = True
            Debug.WriteLine("Scheduler started for " & database & ", next run " & nextRun.ToString("yyyy-MM-dd HH:mm"))
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
        If System.Threading.Interlocked.CompareExchange(tickInProgress, 1, 0) <> 0 Then
            Return
        End If

        Try
            Dim due As Boolean
            SyncLock scheduleLock
                due = DateTime.Now >= nextRun
            End SyncLock

            ' If a manual backup is running, wait for it and check again on the next tick
            If Not due OrElse PostgreSQLBackupManager.IsBackupRunning Then
                Return
            End If

            Dim targets As List(Of String)
            SyncLock scheduleLock
                targets = If(pendingDatabases, PostgreSQLBackupManager.DatabasesFor(currentDatabase).ToList())
            End SyncLock

            Dim failed As New List(Of String)
            For Each database In targets
                If Not PerformBackup(database) Then
                    failed.Add(database)
                End If
            Next

            If failed.Count = 0 Then
                SyncLock scheduleLock
                    nextRun = NextOccurrence(DateTime.Now)
                    pendingDatabases = Nothing
                End SyncLock
                RaiseEvent BackupCompleted()
            Else
                Dim retryAt As DateTime
                SyncLock scheduleLock
                    ' Retry only the failed databases soon, but never later than the next regular backup
                    ' (which backs up everything again)
                    retryAt = DateTime.Now.Add(RetryInterval)
                    pendingDatabases = failed
                    Dim regular = NextOccurrence(DateTime.Now)
                    If regular <= retryAt Then
                        retryAt = regular
                        pendingDatabases = Nothing
                    End If
                    nextRun = retryAt
                End SyncLock
                Debug.WriteLine("Scheduled backup will retry at " & retryAt.ToString("yyyy-MM-dd HH:mm"))
                RaiseEvent BackupFailed(String.Join(", ", failed), retryAt)
            End If
        Catch ex As Exception
            Debug.WriteLine("Timer error: " & ex.Message)
        Finally
            System.Threading.Interlocked.Exchange(tickInProgress, 0)
        End Try
    End Sub

    Private Function NextOccurrence(after As DateTime) As DateTime
        ' First scheduled day/time strictly after the given moment. 62 days always spans a full month.
        Dim backupTimeSpan = TimeSpan.Parse(Me.BackupTime)
        For i = 0 To 62
            Dim candidate = after.Date.AddDays(i).Add(backupTimeSpan)
            If candidate > after AndAlso IsScheduledDay(candidate) Then
                Return candidate
            End If
        Next
        Return DateTime.MaxValue
    End Function

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

    Private Function PerformBackup(database As String) As Boolean
        Try
            Debug.WriteLine("Executing scheduled backup for " & database)
            Dim backupFile = backupManager.CreateBackup(database, currentBackupFolder)

            If Not String.IsNullOrEmpty(backupFile) Then
                Debug.WriteLine("Scheduled backup completed: " & backupFile)
                Return True
            End If
            Debug.WriteLine("Scheduled backup failed")
        Catch ex As Exception
            Debug.WriteLine("Backup execution error: " & ex.Message)
        End Try
        Return False
    End Function
End Class
