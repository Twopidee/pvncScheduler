Imports System.Security.Cryptography
Imports System.Text

' Encrypts the PostgreSQL password with Windows DPAPI so it can be kept in the settings file.
' Only the same Windows user on the same PC can decrypt it.
Public NotInheritable Class PasswordProtector
    Private Shared ReadOnly Entropy As Byte() = Encoding.UTF8.GetBytes("pvncScheduler")

    Public Shared Function Protect(password As String) As String
        If String.IsNullOrEmpty(password) Then
            Return ""
        End If
        Dim encrypted = ProtectedData.Protect(Encoding.UTF8.GetBytes(password), Entropy, DataProtectionScope.CurrentUser)
        Return Convert.ToBase64String(encrypted)
    End Function

    ' Returns Nothing if the value can't be decrypted (e.g. settings copied from another user or PC)
    Public Shared Function Unprotect(protectedValue As String) As String
        If String.IsNullOrEmpty(protectedValue) Then
            Return ""
        End If
        Try
            Dim decrypted = ProtectedData.Unprotect(Convert.FromBase64String(protectedValue), Entropy, DataProtectionScope.CurrentUser)
            Return Encoding.UTF8.GetString(decrypted)
        Catch ex As Exception
            Debug.WriteLine("Could not decrypt saved password: " & ex.Message)
            Return Nothing
        End Try
    End Function
End Class
