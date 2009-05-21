''' <summary>
''' Plugin interface for new list type
''' </summary>
''' <remarks></remarks>
Public Interface IReader

    ReadOnly Property Name() As String
    ReadOnly Property Options() As String


    ''' <summary>
    ''' Is more data available?
    ''' </summary>
    ''' <returns>True if there is more data to enumarate, False otherwise.</returns>
    ''' <remarks></remarks>
    Function ListAvailable() As Boolean

    ''' <summary>
    ''' Start Reading the list
    ''' </summary>
    ''' <remarks>
    ''' Use for initiliase resources, get ready.
    ''' </remarks>
    Sub StartReading(ByVal options As Object)

    ''' <summary>
    ''' Get next item in the list
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function GetNext() As String

    ''' <summary>
    ''' Close / free opened resources if required
    ''' </summary>
    ''' <remarks></remarks>
    Sub Close()



End Interface
