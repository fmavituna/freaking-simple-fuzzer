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

    ''' <summary>
    ''' Attack Finished
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="attackEventArgs"></param>
    ''' <remarks></remarks>
    Public Event AttackFinished(ByVal sender As Object, ByVal attackEventArgs As AttackEventArgs)


    Private _CurrentAttack As String
    ''' <summary>
    ''' Current Attack which will be replaced with mark (X)
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




    Private _uri As Uri

    ''' <summary>
    ''' Uri
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Uri() As Uri
        Get
            _uri = New Uri(Settings.AttackUri.Replace(Settings.MARK, _CurrentAttack))
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


    ''' <summary>
    ''' New attack
    ''' </summary>
    ''' <param name="attackWord"></param>
    ''' <remarks></remarks>
    Public Sub New(ByVal attackWord As String, ByVal settings As Settings)
        Me.CurrentAttack = attackWord
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
            Return Me.Settings.RawPostData.Replace(Settings.MARK, _CurrentAttack)
        End Get
    End Property


End Class
