Imports CommandLine

Imports System.ComponentModel.Composition.Hosting
Imports System.ComponentModel.Composition
Imports FM.FSF.Plugin
Imports CommandLine.Text
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Net

''' <summary>
''' Commandline parameters and settings stored in here
''' </summary>
''' <remarks></remarks>
Public Class FSFArguments
	Inherits CommandLineParser

	''' <summary>
	''' Wordlist Plugins
	''' </summary>
	''' <remarks></remarks>
	<Import(GetType(IReader))> _
	Public FuzzingModules As IEnumerable(Of IReader)

#Region "Settings"

	Private _Settings As Settings

	Public ReadOnly Property Settings() As Settings
		Get
			If _Settings Is Nothing Then
				_Settings = New Settings()
				With _Settings
					.AttackUri = Me.URL
					.RequestTimeout = Me.TimeoutSeconds * 1000
					.RawPostData = Me.RawPostData
				End With

			End If
			Return _Settings
		End Get
	End Property

#End Region

#Region "Help"

	Private Shared ReadOnly HeadingInfo As New HeadingInfo(vbNewLine)

	<HelpOption(HelpText:="Display this help screen.")> _
	Public Function GetUsage() As String
		Dim Help As New HelpText(HeadingInfo)
		With Help
			.AddPreOptionsLine(String.Format("FSF.exe -u http://example.com/?id={0} -m fuzzingmodule -o moduleoptions [options]", Settings.MARK))

			.AddPreOptionsLine(vbNewLine)
			.AddPreOptionsLine("= Available Fuzzing Modules =")
			For Each Plugin As IReader In FuzzingModules
				.AddPreOptionsLine("  " & Plugin.Name.PadRight(30) & "Opt: " & Plugin.Options)
			Next

			.AddPreOptionsLine(vbNewLine)
			.AddPreOptionsLine("= Parameters =")

			Help.AddOptions(Me, "Required")
		End With

		Dim HelpAppend As New StringBuilder(500)
		With HelpAppend
			.AppendLine()
			.AppendLine("= Examples =")
			.AppendLine("SQL Injection fuzzing, hides HTTP status code 200")
			.AppendLine(String.Format("  FSF -u http://example.com/?id={0} -m wordlist -o ""c:\wordlists\sqli.txt"" -h 200", Settings.MARK))

			.AppendLine()
			.AppendLine("Find directories")
			.AppendLine("  FSF -h 404 -o c:\Wordlist\directorynames.txt -u http://example.com/" & Settings.MARK & "/")

			.AppendLine("  FSF -h 404,302 -f c:\Wordlist\filenames.txt -u http://example.com/" & Settings.MARK & ".aspx")

		End With

		Return Help.ToString() & HelpAppend.ToString()
	End Function


#End Region


	''' <summary>
	''' Load plugins
	''' </summary>
	''' <remarks></remarks>
	Public Sub New()
		MyBase.New()

		'Load plugins
		'Dim PluginFolder As String = IO.Path.Combine(My.Application.Info.DirectoryPath, "Plugins")
		Dim PluginFolder As String = My.Application.Info.DirectoryPath

		'Load Plugins
		Dim catalog As New DirectoryCatalog(PluginFolder)
		Dim container As New CompositionContainer(catalog) '
		Dim batch As New CompositionBatch()
		batch.AddPart(Me)

		Try
			container.Compose(batch)
		Catch ex As CompositionException
			Console.WriteLine(ex.ToString())
		End Try
	End Sub

#Region "Target"

	<[Option]("u", "url", HelpText:="Target URL. http://example.com/?param=" & Settings.MARK, required:=True)> _
	  Public URL As String = String.Empty

#End Region

#Region "Attack"

	<[Option]("m", "module", HelpText:="Fuzzing Module", required:=False)> _
	Public _AttackType As String = String.Empty

	''' <summary>
	''' Get the active enumerator
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks></remarks>
	Public ReadOnly Property AttackReader() As IReader
		Get
			Return FuzzingModules.Single(Function(plugin) plugin.Name.ToLowerInvariant = _AttackType.ToLowerInvariant)
		End Get
	End Property

	<[Option]("o", "fuzzing-options", HelpText:="Fuzzing Module Options", required:=False)> _
	Public FuzzingModuleOption As String = String.Empty
#End Region

#Region "Request"

	<[Option](Nothing, "method", HelpText:="HTTP Method (default:GET)", required:=False)> _
	Public HTTPMethod As String = "GET"

	<OptionList(Nothing, "addheader", HelpText:="Add Custom Headers. i.e. ""Referrer=http://example.com"";Header2=Value2", Required:=False, Separator:=";"c)> _
	Public AddHeader As List(Of String)

	<[Option]("f", "two-parameter-fuzzing", HelpText:="Use 2 fuzzing parameters every line should be separated by "":"" [FUZZ], " & Settings.MARK2, required:=False)> _
 Public UseTwoParameterFuzzing As Boolean = False


	<[Option](Nothing, "domain", HelpText:="NTLM Domain if required - Fuzzable", required:=False)> _
 Public AuthenticationDomain As String

	<[Option](Nothing, "username", HelpText:="Authentication Username (NTLM,Basic or Digest) - Fuzzable", required:=False)> _
	Public AuthenticationUser As String

	<[Option](Nothing, "password", HelpText:="Authentication Username (NTLM,Basic or Digest) - Fuzzable", required:=False)> _
	Public AuthenticationPassword As String

	<[Option](Nothing, "disable-proxy", HelpText:="Disable system proxy", required:=False)> _
 Public DisableSystemProxy As Boolean = False

#End Region

#Region "Filtering"

	<OptionList("h", "hide-status", HelpText:="Hide Status Code (seperate status codes, i.e. 404;301)", Required:=False, Separator:=","c)> _
	Public HideStatusCodes As List(Of String)

#End Region

#Region "Capture"

	<[Option]("c", "capture", HelpText:="Regex Capture (i.e : (.*.)) to output a file", required:=False)> _
	Public _Capture As String = Nothing

	Public _CaptureCompiled As RegularExpressions.Regex = Nothing

	''' <summary>
	''' RegularExpression Regex capture
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks></remarks>
	Public ReadOnly Property Capture() As Regex
		Get
			If Not String.IsNullOrEmpty(_Capture) Then
				_CaptureCompiled = New Regex(_Capture, RegexOptions.Compiled Or RegexOptions.Multiline Or RegexOptions.Singleline)
				_Capture = Nothing
			End If

			Return _CaptureCompiled
		End Get
	End Property

	<[Option](Nothing, "capture-output", HelpText:="Regex capture output file (create, append)", required:=False)> _
	Public CaptureOutput As String

	<[Option]("g", "capture-template", HelpText:="Capture template (uses String.Format. {0}=" & Settings.MARK & ", {1}=capture, {2}=New Line, {3}=" & Settings.MARK & ", default:{0} : {1}{2})", required:=False)> _
 Public Template As String = "{0} : {1}{2}"

	<[Option](Nothing, "capture-group", HelpText:="Capture Group Index (only captures the specified group, default:none)", required:=False)> _
   Public CaptureGroup As Integer = -1

	<[Option](Nothing, "match-template", HelpText:="Uses String.Format. Defines output format when you got more than 1 matches.({0}=Match,{1}=New Line, default:""{0},"" )", required:=False)> _
	Public MatchTemplate As String = "{0},"

#End Region

#Region "Connection"

	<[Option]("p", "proxy", HelpText:="Proxy URL", required:=False)> _
	Public Proxy As String

	<[Option]("d", "use-default-proxy", HelpText:="Default Proxy", required:=False)> _
	Public DefaultProxy As Boolean = False

	<[Option]("q", "timeout", HelpText:="Timeout as Seconds (default:60)", required:=False)> _
	 Public TimeoutSeconds As Integer = 60

	<[Option]("t", "thread", HelpText:="Thread Count (default:10)", required:=False)> _
	Public _ThreadCount As Integer = DEFAULT_THREADCOUNT

	Private Const DEFAULT_THREADCOUNT As Integer = 10

	''' <summary>
	''' Maximum Thread Count
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks></remarks>
	Public ReadOnly Property ThreadCount() As Integer
		Get
			If _ThreadCount < 1 Then _ThreadCount = DEFAULT_THREADCOUNT
			Return _ThreadCount
		End Get
	End Property

#End Region


#Region "POST Data"

	'<[Option]("p", "post", HelpText:="Post Data Parameter", required:=False)> _
	'Public PostData As String

	<[Option]("p", "postdata", HelpText:="Raw Post Data", required:=False)> _
	Public RawPostData As String

	<[Option](Nothing, "print-responses", HelpText:="Prints HTTP repsonses to the screen", required:=False)> _
	Public PrintResponses As Boolean
#End Region
End Class

