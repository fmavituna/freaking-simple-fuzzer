Imports System.Net


Public Class AttackEventArgs
    Inherits EventArgs

    Public Sub New()
    End Sub

End Class

''' <summary>
''' One attack 
''' </summary>
''' <remarks></remarks>
Public Class Attack

	Private _disableSystemProxy As Boolean
	Public Property DisableSystemProxy() As Boolean
		Get
			Return _disableSystemProxy
		End Get
		Set(ByVal Value As Boolean)
			_disableSystemProxy = Value
		End Set
	End Property


	''' <summary>
	''' Attack Finished
	''' </summary>
	''' <param name="sender"></param>
	''' <param name="attackEventArgs"></param>
	''' <remarks></remarks>
	Public Event AttackFinished(ByVal sender As Object, ByVal attackEventArgs As AttackEventArgs)


	Private _CurrentAttack As String
	''' <summary>
	''' Current Attack which will be replaced with mark [FUZZ]
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks></remarks>
	Public Property CurrentAttack() As String
		Get
			Return _CurrentAttack
		End Get
		Set(ByVal value As String)
			_CurrentAttack = value
		End Set
	End Property

	Private _HTTPMethod As String

	''' <summary>
	''' HTTP Method
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks></remarks>
	Public Property HTTPMethod() As String
		Get
			If String.IsNullOrEmpty(_HTTPMethod) Then _HTTPMethod = "GET"
			Return _HTTPMethod
		End Get
		Set(ByVal value As String)
			_HTTPMethod = value
		End Set
	End Property

	Private _authUsername As String
	Public Property AuthUsername() As String
		Get
			Return _authUsername
		End Get
		Set(ByVal Value As String)
			_authUsername = Value
		End Set
	End Property

	Private _authPassword As String
	Public Property AuthPassword() As String
		Get
			Return _authPassword
		End Get
		Set(ByVal Value As String)
			_authPassword = Value
		End Set
	End Property

	Private _authDomain As String
	Public Property AuthDomain() As String
		Get
			Return _authDomain
		End Get
		Set(ByVal Value As String)
			_authDomain = Value
		End Set
	End Property



	Private _uri As Uri

	''' <summary>
	''' Uri
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks></remarks>
	Public ReadOnly Property Uri() As Uri
		Get
			_uri = New Uri(ReplaceFuzz(Settings.AttackUri))
			Return _uri
		End Get
	End Property


	Private _Settings As Settings

	''' <summary>
	''' Main Settings
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks></remarks>
	Public Property Settings() As Settings
		Get
			Return _Settings
		End Get
		Set(ByVal value As Settings)
			_Settings = value
		End Set
	End Property


	Private _currentAttack2 As String
	''' <summary>
	''' Current Attack which will be replaced with mark [FUZZ2]
	''' </summary>
	''' <value>The current attack2.</value>
	Public Property CurrentAttack2() As String
		Get
			Return _currentAttack2
		End Get
		Set(ByVal value As String)
			_currentAttack2 = value
		End Set
	End Property
	''' <summary>
	''' New attack
	''' </summary>
	''' <param name="attackWord"></param>
	''' <remarks></remarks>
	Public Sub New(ByVal attackWord As String, ByVal attackWord2 As String, ByVal settings As Settings)
		Me.CurrentAttack = attackWord
		Me.CurrentAttack2 = attackWord2
		Me.Settings = settings
	End Sub

	Private _Response As HttpWebResponse

	''' <summary>
	''' HTTP Web Response
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks></remarks>
	Public Property Response() As HttpWebResponse
		Get
			Return _Response
		End Get
		Set(ByVal value As HttpWebResponse)
			_Response = value
		End Set
	End Property

	''' <summary>
	''' Gets the Network Credentials.
	''' </summary>
	''' <value>The credentials.</value>
	Public ReadOnly Property Credentials() As System.Net.NetworkCredential
		Get
			Return GetCredentials(ReplaceFuzz(Me.AuthUsername), ReplaceFuzz(Me.AuthPassword), ReplaceFuzz(Me.AuthDomain), _Settings)
		End Get
	End Property

	''' <summary>
	''' Add Credentials to supplied Request Settings
	''' </summary>
	''' <param name="UserName"></param>
	''' <param name="Password"></param>
	''' <param name="Domain"></param>
	''' <param name="RequestSettings">Request settings to modify</param>
	''' <remarks></remarks>
	Private Function GetCredentials(ByVal userName As String, ByVal password As String, ByVal domain As String, ByVal requestSettings As Settings) As NetworkCredential

		'Dim ProxyCredentials As Boolean
		'ProxyCredentials = argument.TrimStart("-".ToCharArray).StartsWith("p", StringComparison.InvariantCultureIgnoreCase)

		''Add Credentials
		'If requestSettings.UseDefaultCredentials AndAlso Not ProxyCredentials Then
		'	requestSettings.Credential = CredentialCache.DefaultNetworkCredentials

		'ElseIf requestSettings.UseDefaultCredentialsProxy AndAlso ProxyCredentials Then
		'	requestSettings.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials

		'ElseIf Not String.IsNullOrEmpty(userName) Then

		If Not String.IsNullOrEmpty(userName) Then
			Dim Credentials As New NetworkCredential(userName, password)
			If Not String.IsNullOrEmpty(domain) Then Credentials.Domain = domain

			Return Credentials
		End If


		'If ProxyCredentials Then
		'	requestSettings.Proxy.Credentials = Credentials

		'Else
		'	requestSettings.Credential = Credentials

		'End If

		'End If
		Return Nothing
	End Function


	''' <summary>
	''' Attack
	''' </summary>
	''' <remarks></remarks>
	Public Sub Attack()

		Dim NewWebRequest As WebRequest = Request.GenerateWebRequest(Me)
		Dim Response As HttpWebResponse = Request.DoRequest(NewWebRequest, _SourceCode)

		Me.Response = Response
		RaiseEvent AttackFinished(Me, Nothing)
		'Return Response
	End Sub


	Private _SourceCode As String
	Public ReadOnly Property SourceCode() As String
		Get
			Return _SourceCode
		End Get
	End Property

	Public PostParameters As New Dictionary(Of String, String)
	Public FileParameters As New Dictionary(Of String, String)

	''' <summary>
	''' Gets or sets the raw post data.
	''' </summary>
	''' <value>The raw post data.</value>
	Public ReadOnly Property RawPostData() As String
		Get
			If String.IsNullOrEmpty(Me.Settings.RawPostData) Then Return String.Empty
			Return ReplaceFuzz(Me.Settings.RawPostData)
		End Get
	End Property


	''' <summary>
	''' Replaces the [FUZZ] and [FUZZ2] marks.
	''' </summary>
	''' <param name="input">The input.</param>
	''' <returns></returns>
	Public Function ReplaceFuzz(ByVal input As String) As String
		If String.IsNullOrEmpty(input) Then Return input
		input = input.Replace(Settings.MARK, Me.CurrentAttack)
		input = input.Replace(Settings.MARK2, Me.CurrentAttack2)

		Return input
	End Function


End Class
