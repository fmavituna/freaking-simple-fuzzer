Imports System.IO
Imports FM.FSF.Plugin
Imports System.ComponentModel.Composition

''' <summary>
''' Read a file from the file system (seperated by newlines)
''' </summary>
''' <remarks></remarks>
<Export(GetType(IReader))> _
Public Class Wordlist
    Implements IReader


    Private FileName As String

    Public Sub Close() Implements IReader.Close
        FileStream.Close()
    End Sub

    Dim FileStream As StreamReader

    Public Function GetNext() As String Implements IReader.GetNext
        Return FileStream.ReadLine()
    End Function

    Public Sub StartReading(ByVal options As Object) Implements IReader.StartReading
        Me.FileName = DirectCast(options, String)
        FileStream = New StreamReader(FileName)
    End Sub

    Public Function ListAvailable() As Boolean Implements IReader.ListAvailable
        Return Not FileStream.EndOfStream
    End Function

    Public ReadOnly Property Name() As String Implements Plugin.IReader.Name
        Get
            Return "Wordlist"
        End Get
    End Property

    Public ReadOnly Property Options() As String Implements FM.FSF.Plugin.IReader.Options
        Get
            Return "File Path i.e. (c:\wordlist\list.txt)"
        End Get
    End Property
End Class
