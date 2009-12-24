Imports System.Net

''' <summary>
''' UriManager Settings
''' </summary>
''' <remarks></remarks>
Public Class Settings

#Region "Constants"

    Const ConstRequestTimeout As Integer = 60000

#End Region

	Public Const MARK As String = "[FUZZ]"
	Public Const MARK2 As String = "[FUZZ2]"

    Private _AttackUri As String


    ''' <summary>
    ''' Attack Uri which includes (X) mark
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property AttackUri() As String
        Get
            Return _AttackUri
        End Get
        Set(ByVal value As String)
            _AttackUri = value
        End Set
    End Property


    Private _Proxy As WebProxy
    ''' <summary>
    ''' Proxy Settings
    ''' </summary>
    ''' <value>The proxy.</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Proxy() As WebProxy
        Get
            Return _Proxy
        End Get
        Set(ByVal value As WebProxy)
            _Proxy = value
        End Set
    End Property


    Private _proxyEnabled As Boolean

    ''' <summary>
    ''' Gets or sets a value indicating whether Proxy Enabled ?.
    ''' </summary>
    ''' <value><c>true</c> if Proxy Enabled; otherwise, <c>false</c>.</value>
    Public Property ProxyEnabled() As Boolean
        Get
            Return _proxyEnabled
        End Get
        Set(ByVal value As Boolean)
            _proxyEnabled = value
        End Set
    End Property


    Private _userAgentString As String

    ''' <summary>
    ''' Gets or sets the user agent string.
    ''' </summary>
    ''' <value>The user agent string.</value>
    Public Property UserAgentString() As String
        Get
            Return _userAgentString
        End Get
        Set(ByVal value As String)
            _userAgentString = value
        End Set
    End Property

    Private _Credential As NetworkCredential
    ''' <summary>
    ''' Gets or sets Network Credentials
    ''' </summary>
    ''' <value>The Network Credential.</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Credential() As NetworkCredential
        Get
            Return _Credential
        End Get
        Set(ByVal value As NetworkCredential)
            _Credential = value
        End Set
    End Property

    Private _RequestTimeout As Integer
    ''' <summary>
    ''' Gets or sets the request timeout as miliseconds.
    ''' </summary>
    ''' <value>The request timeout.</value>
    Public Property RequestTimeout() As Integer
        Get
            If _RequestTimeout = 0 Then _RequestTimeout = ConstRequestTimeout
            Return _RequestTimeout
        End Get
        Set(ByVal value As Integer)
            _RequestTimeout = value
        End Set
    End Property


    Private _Headers As Dictionary(Of String, String)
    ''' <summary>
    ''' Gets or sets the Http Headers.
    ''' </summary>
    ''' <value>The Http Header.</value>
    Public Property Headers() As Dictionary(Of String, String)
        Get
            If _Headers Is Nothing Then _Headers = New Dictionary(Of String, String)
            Return _Headers
        End Get
        Set(ByVal value As Dictionary(Of String, String))
            _Headers = value
        End Set
    End Property



    ''' <summary>
    ''' Adds the header.
    ''' </summary>
    ''' <param name="Name">The name.</param>
    ''' <param name="Value">The value.</param>
    ''' <returns></returns>
    Public Function AddHeader(ByVal name As String, ByVal value As String) As Boolean
        If Headers.ContainsKey(name) Then
            Return False

        Else
            Headers.Add(name, value)
            Return True

        End If

    End Function

    Private _UseDefaultCredentials As Boolean
    ''' <summary>
    ''' Gets or sets a value indicating whether request should use default credentials.
    ''' </summary>
    ''' <value>
    ''' <c>true</c> if request uses default credentials; otherwise, <c>false</c>.
    ''' </value>
    ''' <remarks>Current application logged user credentials </remarks>
    Public Property UseDefaultCredentials() As Boolean
        Get
            Return _UseDefaultCredentials
        End Get
        Set(ByVal value As Boolean)
            _UseDefaultCredentials = value
        End Set
    End Property

    Private _UseDefaultCredentialsProxy As Boolean
    ''' <summary>
    ''' Gets or sets a value indicating whether request should use default credentials.
    ''' </summary>
    ''' <value>
    ''' <c>true</c> if request uses default credentials; otherwise, <c>false</c>.
    ''' </value>
    ''' <remarks>Current application logged user credentials </remarks>
    Public Property UseDefaultCredentialsProxy() As Boolean
        Get
            Return _UseDefaultCredentialsProxy
        End Get
        Set(ByVal value As Boolean)
            _UseDefaultCredentialsProxy = value
        End Set
    End Property


    Private _Capture As Text.RegularExpressions.Regex
    ''' <summary>
    ''' RegularExpression Regex capture
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Capture() As Text.RegularExpressions.Regex
        Get
            Return _Capture
        End Get
        Set(ByVal value As Text.RegularExpressions.Regex)
            _Capture = value
        End Set
    End Property

    Public Enum Readers
        WordList = 0
        NumericList = 1
    End Enum

    Private _Reader As Readers
    Public Property Reader() As Readers
        Get
            Return _Reader
        End Get
        Set(ByVal value As Readers)
            _Reader = value
        End Set
    End Property

    Private _RawPostData As String

    ''' <summary>
    ''' Gets or sets the post data.
    ''' </summary>
    ''' <value>The post data.</value>
    Public Property RawPostData() As String
        Get
            Return _RawPostData
        End Get
        Set(ByVal Value As String)
            _RawPostData = Value
        End Set
    End Property

End Class
