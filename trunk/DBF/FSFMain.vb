Imports FM.FSF.Plugin
Imports System.Threading
Imports System.Net
Imports System.Xml
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions

Imports CommandLine


#Region "Credits"
'Ferruh Mavituna ( ferruh-at-mavituna.com | http://ferruh.mavituna.com )
'
'EXTERNAL LIBRARIES:
'http://commandline.codeplex.com/
#End Region

#Region "License"
'GPL v3
#End Region

''' <summary>
''' Ferruh Mavituna's Freaking Simple Fuzzer
''' </summary>
''' <remarks></remarks>
Module FSFMain

#Region "Usage"
    ''' <summary>
    ''' Print Banner
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub PrintBanner()
        Console.WriteLine()

        'Look ma, l33t ASCII art!
        Dim Middle As Integer = CInt((Console.WindowWidth) / 2) + 5
        Console.WriteLine("     ___     ___ ".PadLeft(Middle))
        Console.WriteLine("    |  _|___|  _|".PadLeft(Middle))
        Console.WriteLine("    |  _|_ -|  _|".PadLeft(Middle))
        Console.WriteLine("    |_| |___|_|  ".PadLeft(Middle))
        Console.Write(("Freaking Simple Fuzzer v" & My.Application.Info.Version.ToString).PadLeft(Middle + 10))
    End Sub

    Public Sub PrintUsage()
        Console.WriteLine(Options.GetUsage())
    End Sub


#End Region

#Region "Properties"

    Private FoundCounter, GenericCounter As Integer

    Private _ThreadPool As ThreadPool

    ''' <summary>
    ''' Thread Pool
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property ThreadPool() As ThreadPool
        Get
            If _ThreadPool Is Nothing Then _ThreadPool = New ThreadPool(Options.ThreadCount)
            Return _ThreadPool
        End Get
    End Property

    Private _HideStatus As New List(Of Integer)

    ''' <summary>
    ''' HTTP Status Messages to not show
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property HideStatus() As List(Of Integer)
        Get
            'Add 404 by default if it's empty
            If _HideStatus.Count = 0 Then _HideStatus.Add(404)
            Return _HideStatus
        End Get
        Set(ByVal value As List(Of Integer))
            _HideStatus = value
        End Set
    End Property

    ''' <summary>
    ''' Console Lock Object for thread-safety and sync
    ''' </summary>
    ''' <remarks></remarks>
    Private ConsoleLock As New Object


    ''' <summary>
    ''' Progress bar animation
    ''' </summary>
    ''' <remarks></remarks>
    Private Progress() As String = {"-", "\", "|", "/", "*"}

#End Region

#Region "Entry Point"

    Dim Options As FSFArguments

    ''' <summary>
    ''' Mains this instance.
    ''' </summary>
    Sub Main(ByVal args() As String)
        PrintBanner()

        'Load Arguments
        Options = New FSFArguments()
        Dim Parser As New CommandLineParser()

        If Not (Parser.ParseArguments(args, Options, Console.Out)) Then
			ErrorQuit("Supplied arguments could not parsed.", ErrorCode.InvalidArgument)
        End If

		Start()

    End Sub

#End Region

#Region "Core Flow and Real Action"

    ''' <summary>
    ''' Start real action
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub Start()

        Console.Clear()

        AddHandler ThreadPool.ThreadsFinished, AddressOf HandleFinished
		Dim ListReader As IReader = Options.AttackReader()

		If ListReader Is Nothing Then
			ErrorQuit("Coudln't find the selected module. You need to choose a valid fuzzing module. Use -m and supply a valid module name.", ErrorCode.Incorrect)
		End If

		Try
			ListReader.StartReading(Options.FuzzingModuleOption)

		Catch ex As Exception
			ErrorQuit(String.Format("Error during the ""{0}"" module start", ListReader.Name), ErrorCode.Incorrect)

		End Try


		While ListReader.ListAvailable


			Dim fuzz2 As String = String.Empty
			Dim currentFuzzWord As String = ListReader.GetNext()
			Dim fuzz1 As String = currentFuzzWord

			'Handle two parameter fuzzing
			If Options.UseTwoParameterFuzzing Then
				Dim splitpos As Integer = currentFuzzWord.IndexOf(":"c)
				If splitpos > -1 Then
					fuzz1 = currentFuzzWord.Substring(0, splitpos)
					fuzz2 = currentFuzzWord.Substring(splitpos + 1)
				End If
			End If


			Dim Attack As New Attack(fuzz1, fuzz2, Options.Settings)
			With Attack
				.AuthUsername = Options.AuthenticationUser
				.AuthPassword = Options.AuthenticationPassword
				.AuthDomain = Options.AuthenticationDomain

				.HTTPMethod = Options.HTTPMethod
			End With

			AddHandler Attack.AttackFinished, AddressOf Attacked

			ThreadPool.Open()

			Dim AttackJob As New Thread(AddressOf Attack.Attack)
			AttackJob.Start()

			ThreadPool.WaitForThreads()

		End While
		ListReader.Close()

		ThreadPool.AllJobsPushed()

		If Console.CursorLeft = 0 Then Console.WriteLine(" ".PadRight(Console.WindowWidth - 1))

    End Sub

    ''' <summary>
    ''' Attacks finished
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub HandleFinished()
        SyncLock ConsoleLock
            Console.WriteLine()
            Console.WriteLine()
            Console.Write(New String("-"c, Console.WindowWidth))
            Console.WriteLine("Attack Finished!")
        End SyncLock
    End Sub

    ''' <summary>
    ''' Write Lock for capture output file
    ''' </summary>
    ''' <remarks></remarks>
    Private OutputWriteLock As New Object()

    ''' <summary>
    ''' One attack finished
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="attackEventArgs"></param>
    ''' <remarks></remarks>
    Private Sub Attacked(ByVal sender As Object, ByVal attackEventArgs As AttackEventArgs)

        Dim Attack As Attack = DirectCast(sender, Attack)

        SyncLock ConsoleLock

            If Attack.Response Is Nothing Then
                Console.Error.WriteLine("Error in Response ")
                ThreadPool.Close()
                Exit Sub
            End If

            'Output
			Dim attackWord As String = Attack.CurrentAttack
			If Not String.IsNullOrEmpty(Attack.CurrentAttack2) Then attackWord = String.Format("{0}:{1}", Attack.CurrentAttack, Attack.CurrentAttack2)

			Dim Result As String = String.Format("{0}{1}{2}", Attack.Uri.PathAndQuery.PadRight(50), Convert.ToInt16(Attack.Response.StatusCode).ToString.PadRight(5), attackWord.ToString.PadRight(10))

			Console.CursorLeft = 0

			If ValidHTTPStatus(Attack.Response.StatusCode) Then
				Interlocked.Increment(FoundCounter)

				If Options.PrintResponses Then Console.WriteLine(Attack.SourceCode)

				Dim CaptureResult As String = String.Empty

				If Options.Capture IsNot Nothing Then
					For Each CapData As Match In Options.Capture.Matches(Attack.SourceCode)
						Dim CapValue As String = CapData.Value
						If Options.CaptureGroup > -1 AndAlso (CapData.Groups.Count > Options.CaptureGroup) Then CapValue = CapData.Groups(Options.CaptureGroup).Value
						CaptureResult &= String.Format(Options.MatchTemplate, CapValue, vbNewLine)
					Next

				End If

				If Not String.IsNullOrEmpty(Options.CaptureOutput) Then
					SyncLock OutputWriteLock
						My.Computer.FileSystem.WriteAllText(Options.CaptureOutput, String.Format(Options.Template, Attack.CurrentAttack, CaptureResult, vbNewLine, Attack.CurrentAttack2), True)
					End SyncLock
				End If

				Console.WriteLine(Result)

			Else
				Console.Write(Progress((GenericCounter - FoundCounter) Mod Progress.Length).ToString.PadRight(50))
				Console.Write(Convert.ToInt16(Attack.Response.StatusCode).ToString)
				Console.Write(" - ")
				Console.Write(GenericCounter.ToString)

			End If

		End SyncLock

        Interlocked.Increment(GenericCounter)

        ThreadPool.Close()
    End Sub

#End Region


#Region "Helpers"


    ''' <summary>
    ''' Quit from application
    ''' </summary>
    ''' <remarks></remarks>
	Private Sub Quit(ByVal code As Integer)
		Environment.Exit(ErrorCode.Successfull)
	End Sub

    ''' <summary>
    ''' Valid HTTP Status
    ''' </summary>
    ''' <param name="status"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function ValidHTTPStatus(ByVal status As HttpStatusCode) As Boolean
        If Options.HideStatusCodes Is Nothing Then Return True

        Return Not Options.HideStatusCodes.IndexOf(Convert.ToInt16(status).ToString()) > -1
    End Function

    ''' <summary>
    ''' Write error to screen and quit
    ''' </summary>
    ''' <param name="message"></param>
    ''' <remarks></remarks>
	Public Sub ErrorQuit(ByVal message As String, ByVal errorCode As ErrorCode)
		Console.Error.WriteLine(message)
		Quit(errorCode)
	End Sub

#End Region

End Module


'TODO: Add Post Parameter support to arguments with optional HTTP Encode on the fly
'TODO: Add attack modifiers (scuh as base64 etc.), it should be plugin based as well just like attack modules.