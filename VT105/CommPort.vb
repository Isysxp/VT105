Imports System
Imports System.Collections.Generic
Imports System.Text
Imports System.IO.Ports
Imports System.Diagnostics


Public Class CommPort
    Private Cport As SerialPort
    Private Dsync As Integer


    Public Sub New(ByVal Port As String)
        Cport = New SerialPort(Port)
        Cport.BaudRate = 115200
        Cport.DtrEnable = True
        Cport.RtsEnable = True
        Cport.Open()
        Dsync = 0
    End Sub

    Public Sub WriteLine(ByVal cmd As String)
        Cport.Write(cmd & vbCrLf)
    End Sub

    Public Sub Write(ByVal cmd As String)
        Cport.Write(cmd)
    End Sub

    Public Sub WriteChar(ByVal Chr As Char)
        Cport.Write(Chr)
    End Sub

    Public Function Read() As String
        If Not Cport.IsOpen Then
            Return Nothing
        End If

        'Dim sb As StringBuilder = New StringBuilder()
        Dim sb As String = ""
        Dim Bfr As String
        Try
            If Cport.BytesToRead > 0 Then
                Bfr = Cport.ReadExisting
                'Dim Bfr(Cport.BytesToRead + 10) As Byte
                'Dtr = Cport.Read(Bfr, 0, Cport.BytesToRead)
                'sb.Append(Encoding.Default.GetString(Bfr))
                sb = sb & Bfr
                System.Threading.Thread.Sleep(1)
            End If
        Catch ex As Exception
            MsgBox("The serial port has closed. Click OK to retry....", MsgBoxStyle.Critical, "VT105")
            Try
                Cport.Close()

            Catch exa As Exception

            End Try
            Cport.Open()
            Return ""
        End Try
        Dsync += 1
        If Dsync > 10 Then
            UpdateDisplay()
            Dsync = 0
        End If
        Return sb
    End Function

    Public ReadOnly Property IsConnected() As Boolean
        Get
            Return Cport.IsOpen
        End Get
    End Property

End Class
