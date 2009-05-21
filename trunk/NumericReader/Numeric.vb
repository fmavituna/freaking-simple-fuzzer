Imports System.IO
Imports FM.FSF.Plugin
Imports System.ComponentModel.Composition

''' <summary>
''' Generate integers in memory and feed it back to reader.
''' </summary>
''' <remarks></remarks>
<Export(GetType(IReader))> _
Public Class NumericList
    Implements IReader


    Private Increaser As Long = 1
    Private CurrentNumber, StartNumber, EndNumber As Long

    Public Sub Close() Implements IReader.Close
    End Sub

    Public Function GetNext() As String Implements IReader.GetNext
        CurrentNumber += Increaser
        Return CurrentNumber.ToString()
    End Function

    Public Sub StartReading(ByVal options As Object) Implements IReader.StartReading
        Dim SplitOptions() As String = DirectCast(options, String).Split("-"c)
        StartNumber = CType(SplitOptions(0), Long)
        EndNumber = CType(SplitOptions(1), Long)

        If SplitOptions.Length > 2 Then Increaser = CType(SplitOptions(2), Long)
    End Sub

    Public Function ListAvailable() As Boolean Implements IReader.ListAvailable
        Return CurrentNumber <= EndNumber
    End Function

    Public ReadOnly Property Name() As String Implements FM.FSF.Plugin.IReader.Name
        Get
            Return "Integer"
        End Get
    End Property

    Public ReadOnly Property Options() As String Implements FM.FSF.Plugin.IReader.Options
        Get
            Return "StartNumber-EndNumber[-Increaser] i.e. (1,1000)"
        End Get
    End Property
End Class
