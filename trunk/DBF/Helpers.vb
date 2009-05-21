Imports System.Net.Security
Imports System.Net

Public Module Helpers

    ''' <summary>
    ''' Validates the certificates (accept any of them).
    ''' </summary>
    ''' <param name="sender">The sender.</param>
    ''' <param name="certificate">The certificate.</param>
    ''' <param name="chain">The chain.</param>
    ''' <param name="sslPolicyErrors">The SSL policy errors.</param>
    ''' <returns><c>Always True</c></returns>
    Public Function ValidateCertificate(ByVal sender As Object, ByVal certificate As System.Security.Cryptography.X509Certificates.X509Certificate, ByVal chain As System.Security.Cryptography.X509Certificates.X509Chain, ByVal sslPolicyErrors As SslPolicyErrors) As Boolean
        'Accept any certificate
        Return True
    End Function

    ''' <summary>
    ''' Parses the specified name.
    ''' </summary>
    ''' <param name="RawString">The raw string.</param>
    ''' <param name="Name">The name.</param>
    ''' <param name="Value">The value.</param>
    Public Sub ParseParameter(ByVal RawString As String, ByRef Name As String, ByRef Value As String, Optional ByVal Identifier As Char = "="c)
        Dim ValPos As Integer = RawString.IndexOf(Identifier)

        'Not Found
        If ValPos = -1 Then Exit Sub

        Name = RawString.Substring(0, ValPos)
        Value = RawString.Substring(ValPos + 1, RawString.Length - ValPos - 1)
    End Sub

End Module