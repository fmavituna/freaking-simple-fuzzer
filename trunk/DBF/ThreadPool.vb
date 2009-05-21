Imports System.Threading

''' <summary>
''' Pretty simple Thread Pool implementation
''' </summary>
''' <remarks></remarks>
Public Class ThreadPool

#Region "Properties"
    Private _Count As Integer

    ''' <summary>
    ''' Current Thread Count 
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property ThreadCount() As Integer
        Get
            SyncLock Me
                Return _Count
            End SyncLock
        End Get
    End Property

    Private _MaxThreadCount As Integer

    ''' <summary>
    ''' Maximum Thread Count
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MaxThreadCount() As Integer
        Get
            Return _MaxThreadCount
        End Get
        Set(ByVal value As Integer)
            _MaxThreadCount = value
        End Set
    End Property

    ''' <summary>
    ''' New Thread Pool
    ''' </summary>
    ''' <param name="maxThreadCount"></param>
    ''' <remarks></remarks>
    Public Sub New(ByVal maxThreadCount As Integer)
        Me.MaxThreadCount = maxThreadCount
    End Sub



    ''' <summary>
    ''' Is job finished (all threads closed?)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks><c>True</c> if all opened threads closed <c>False</c> otherwise</remarks>
    Public ReadOnly Property Finished() As Boolean
        Get
            Return AllPushed AndAlso ThreadCount = 0
        End Get
    End Property


    Private _Status As ThreadStatus

    ''' <summary>
    ''' Thread Status
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Status() As ThreadStatus
        Get
            Return _Status
        End Get
        Set(ByVal value As ThreadStatus)
            If _Status = value Then Return
            _Status = value
        End Set
    End Property

    Private _AllPushed As Boolean

    ''' <summary>
    ''' All jobs are pushed to ThreadPool
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private ReadOnly Property AllPushed() As Boolean
        Get
            SyncLock Me
                Return _AllPushed
            End SyncLock
        End Get
    End Property


#End Region

#Region "Enums"
    ''' <summary>
    ''' Current status of thread pool
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum ThreadStatus
        Started
        Stopped
        Paused
    End Enum
#End Region

#Region "Events"
    ''' <summary>
    ''' Inform that there is no more thread in the pool
    ''' </summary>
    ''' <remarks></remarks>
    Public Event ThreadsFinished()
#End Region

#Region "Thread Management"
    ''' <summary>
    ''' Open a new thread
    ''' </summary>
    Public Sub Open()
        Interlocked.Increment(_Count)
    End Sub

    ''' <summary>
    ''' Informs Thread Pool that all jobs pushed
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub AllJobsPushed()
        Me._AllPushed = True
    End Sub


    ''' <summary>
    ''' Close one thread
    ''' </summary>
    Public Sub Close()
        Interlocked.Decrement(_Count)
        If Me.Finished Then RaiseEvent ThreadsFinished()
    End Sub

    ''' <summary>
    ''' Wait for opened threads before open new ones
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WaitForThreads()
        'Wait if there is no empty thread slots
        'Wait if thread is paused
        SyncLock Me
            While (Me.ThreadCount >= MaxThreadCount) OrElse (Me.Status = ThreadStatus.Paused)
                Threading.Thread.Sleep(50)
            End While
        End SyncLock
    End Sub

#End Region

End Class