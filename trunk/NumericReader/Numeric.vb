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

	Private _integerFormat As String
	''' <summary>
	''' Gets the integer for outputs ToString()
	''' </summary>
	''' <value>The integer format.</value>
	Public ReadOnly Property IntegerFormat() As String
		Get
			Return _integerFormat
		End Get
	End Property



    Private Increaser As Long = 1
    Private CurrentNumber, StartNumber, EndNumber As Long

    Public Sub Close() Implements IReader.Close
    End Sub

    Public Function GetNext() As String Implements IReader.GetNext
		Dim retNumber As Long = (CurrentNumber + StartNumber)

		'Increase it afterwards to avoid starting from +1
		CurrentNumber += Increaser

		Return retNumber.ToString(_integerFormat)
    End Function

    Public Sub StartReading(ByVal options As Object) Implements IReader.StartReading
		Dim currentOptions As String = DirectCast(options, String)

		Dim SplitOptions() As String = currentOptions.Split("-"c)
		StartNumber = CType(SplitOptions(0), Long)
        EndNumber = CType(SplitOptions(1), Long)

		'Increaser
		If SplitOptions.Length > 2 Then Increaser = CType(SplitOptions(2), Long)

		'Last parameter is format string
		If SplitOptions.Length > 3 Then _integerFormat = SplitOptions(3)
	End Sub

    Public Function ListAvailable() As Boolean Implements IReader.ListAvailable
		Return (CurrentNumber + StartNumber) <= EndNumber
    End Function

    Public ReadOnly Property Name() As String Implements FM.FSF.Plugin.IReader.Name
        Get
            Return "Integer"
        End Get
    End Property

    Public ReadOnly Property Options() As String Implements FM.FSF.Plugin.IReader.Options
        Get
			Return "StartNumber-EndNumber[-Increaser][-Format] i.e. (1,1000)"
        End Get
    End Property
End Class
