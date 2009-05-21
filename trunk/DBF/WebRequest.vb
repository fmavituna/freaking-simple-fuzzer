Imports System.Net
Imports System.Web
Imports System.IO
Imports System.Text

''' <summary>
''' Manages Web Requests
''' </summary>
''' <remarks></remarks>
Public Class Request

    Private Shared HTTPSettingsConfigured As Boolean = SetHttpSettings()

    Private Shared Function SetHttpSettings() As Boolean
        'TODO: Can we extract these?
        ServicePointManager.ServerCertificateValidationCallback = New Net.Security.RemoteCertificateValidationCallback(AddressOf ValidateCertificate)

        Net.ServicePointManager.DefaultConnectionLimit = 20
        Net.ServicePointManager.UseNagleAlgorithm = True
    End Function


    ''' <summary>
    ''' Gets the post data if there returns RawPostData use it otherwise uses PostParameters.
    ''' </summary>
    ''' <param name="attack">The attack.</param>
    ''' <returns></returns>
    Private Shared Function GetPostData(ByVal attack As Attack) As String
        If Not String.IsNullOrEmpty(attack.RawPostData) Then Return attack.RawPostData
        Return GeneratePostData(attack.PostParameters)
    End Function
    ''' <summary>
    ''' Generate WebRequest with supplied setting
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function GenerateWebRequest(ByVal Attack As Attack) As WebRequest

        Dim WebReq As HttpWebRequest
        Try
            WebReq = CType(HttpWebRequest.Create(Attack.Uri), HttpWebRequest)

        Catch ex As NotSupportedException
            Debug.Assert(False, "URI is not valid handle gracefully pls..")
            Return Nothing

        End Try

        WebReq.AllowAutoRedirect = False

        'Add Credentials
        If Attack.Settings.Credential IsNot Nothing Then
            WebReq.Credentials = Attack.Settings.Credential
        End If

        'Check Proxy
        If Attack.Settings.ProxyEnabled Then

            'If there is proxy use otherwise use IE proxy
            If Attack.Settings.Proxy IsNot Nothing Then
                WebReq.Proxy = Attack.Settings.Proxy
            End If

        Else
            WebReq.Proxy = Nothing

        End If

        'Add Headers
        For Each Header As KeyValuePair(Of String, String) In Attack.Settings.Headers
            Try
                WebReq.Headers.Add(Header.Key, Header.Value)

            Catch ex As ArgumentException
                'TODO: Fix here to find appropriate header and add it to normal ways to able users to ue injections in User-Agent and similiar fields
                Debug.WriteLine("ERROR : This Header can not be added by headers use : " & Header.Key)
                Return Nothing

            End Try
        Next Header


        'Request Timeout
        WebReq.Timeout = Attack.Settings.RequestTimeout

        If Attack.FileParameters.Count > 0 Then
            Dim Boundary As String = "----------------------------" + DateTime.Now.Ticks.ToString("x")
            WebReq.Method = "POST"
            WebReq.ContentType = "multipart/form-data; boundary=" & Boundary
            WebReq.KeepAlive = True

            Dim memStream As New System.IO.MemoryStream()

            Dim boundarybytes As Byte() = System.Text.Encoding.ASCII.GetBytes(vbNewLine & "--" & Boundary & vbNewLine)
            Dim formdataTemplate As String = vbNewLine & "--" + Boundary & vbNewLine & "Content-Disposition: form-data; name=""{0}"";" & vbNewLine & vbNewLine & "{1}"

            For Each PostParameter As KeyValuePair(Of String, String) In Attack.PostParameters

                Dim formitem As String = String.Format(formdataTemplate, PostParameter.Key, PostParameter.Value)
                Dim formitembytes As Byte() = System.Text.Encoding.UTF8.GetBytes(formitem)
                memStream.Write(formitembytes, 0, formitembytes.Length)

            Next

            memStream.Write(boundarybytes, 0, boundarybytes.Length)

            Dim headerTemplate As String = "Content-Disposition: form-data; name=""{0}""; filename=""{1}""" & vbNewLine & " Content-Type: application/octet-stream" & vbNewLine & vbNewLine

            For Each FileParameter As KeyValuePair(Of String, String) In Attack.FileParameters

                'No file to upload, Skip
                If Not IO.File.Exists(FileParameter.Value) Then Continue For


                'TODO: Change this to support multiple files with different input names
                Dim header As String = String.Format(headerTemplate, FileParameter.Key, FileParameter.Value)

                Dim headerbytes As Byte() = System.Text.Encoding.UTF8.GetBytes(header)

                memStream.Write(headerbytes, 0, headerbytes.Length)

                Dim fileStreamx As New FileStream(FileParameter.Value, FileMode.Open, FileAccess.Read)
                Dim buffer(1024 - 1) As Byte 'byte[] buffer = new byte[1024];

                Dim bytesRead As Integer = -1

                While (bytesRead <> 0)
                    bytesRead = fileStreamx.Read(buffer, 0, buffer.Length)
                    memStream.Write(buffer, 0, bytesRead)
                End While


                memStream.Write(boundarybytes, 0, boundarybytes.Length)
                fileStreamx.Close()

            Next


            WebReq.ContentLength = memStream.Length

            Dim requestStream As Stream = WebReq.GetRequestStream()

            memStream.Position = 0

            Dim tempBuffer(CInt(memStream.Length - 1)) As Byte
            memStream.Read(tempBuffer, 0, tempBuffer.Length)
            memStream.Close()

            requestStream.Write(tempBuffer, 0, tempBuffer.Length)
            requestStream.Close()


        ElseIf HasPostData(Attack) Then
            'TODO: If user forced to another method use it? Even though that's a stupid thing to do.
            WebReq.Method = "POST"


            Dim PostData As String = GetPostData(Attack)

            WebReq.ContentType = "application/x-www-form-urlencoded"
            WebReq.ContentLength = PostData.Length

            Dim ReqStream As Stream = Nothing
            Try
                ReqStream = WebReq.GetRequestStream()
                Dim PostByte As Byte() = Encoding.ASCII.GetBytes(PostData)
                ReqStream.Write(PostByte, 0, PostByte.Length)

            Catch WebEx As Net.WebException
                Console.Error.WriteLine("Form stream write error !")

            Finally
                If ReqStream IsNot Nothing Then ReqStream.Close()

            End Try

        Else
            'GET / POST
            WebReq.Method = Attack.HTTPMethod
            WebReq.KeepAlive = True

        End If

        Return WebReq
    End Function

    ''' <summary>
    ''' Generate Post Data for request from PostParameters
    ''' </summary>
    ''' <returns>Suitable postdata stream for a WebRequest</returns>
    ''' <remarks></remarks>
    Public Shared Function GeneratePostData(ByVal postParameters As Dictionary(Of String, String)) As String
        Dim PostData As String = String.Empty

        'Generate Post Data
        For Each PostParam As KeyValuePair(Of String, String) In postParameters
            PostData &= PostParam.Key & "=" & HttpUtility.UrlEncode(PostParam.Value) & "&"
        Next PostParam

        'Fix latest (&)
        If Right(PostData, 1) = "&" Then PostData = PostData.Remove(PostData.Length - 1, 1)

        Return PostData
    End Function



    ''' <summary>
    ''' Make a request
    ''' </summary>
    ''' <remarks></remarks>
    Public Shared Function DoRequest(ByVal webRequest As WebRequest, ByRef SourceCode As String) As HttpWebResponse

        Dim HttpReq As HttpWebRequest = CType(webRequest, HttpWebRequest)

        'Exit by default in exception
        Dim ExitFunction As Boolean = True
        Dim HttpRes As HttpWebResponse = Nothing
        Dim requestTime As DateTime = DateTime.Now

        Try
            HttpRes = CType(HttpReq.GetResponse(), HttpWebResponse)

            'Handle Exceptions
        Catch WebEx As Net.WebException

            'TODO : Handle Errors properly...
            Select Case WebEx.Status
                Case WebExceptionStatus.Timeout
                    Debug.WriteLine("Timeout Error!")

                Case WebExceptionStatus.ProtocolError 'Status 500, 401 etc.
                    HttpRes = DirectCast(WebEx.Response, HttpWebResponse)
                    ExitFunction = False

                Case WebExceptionStatus.ConnectFailure
                    Debug.WriteLine("Connection Error !")

                Case Else
                    Debug.WriteLine("**************************************************")
                    Debug.WriteLine(WebEx.ToString)

            End Select

            'Exit if required
            If ExitFunction Then Return Nothing

        Catch Ex As Exception
            Debug.WriteLine("**************************************************")
            Debug.WriteLine(Ex.ToString())

        End Try 'Web Exception

        Using HReader As New IO.StreamReader(HttpRes.GetResponseStream)
            SourceCode = HReader.ReadToEnd()
        End Using


        HttpRes.Close()

        Return HttpRes
    End Function

    ''' <summary>
    ''' Determines whether the attack has post data or not.
    ''' </summary>
    ''' <param name="attack">The attack.</param>
    ''' <returns>
    ''' <c>true</c> if the attack has post data <c>true</c>; otherwise, <c>false</c>.
    ''' </returns>
    Private Shared Function HasPostData(ByVal attack As Attack) As Boolean
        Return attack.PostParameters.Count > 0 OrElse Not String.IsNullOrEmpty(attack.RawPostData)
    End Function

End Class
