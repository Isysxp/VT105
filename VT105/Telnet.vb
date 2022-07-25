Imports System
Imports System.Collections.Generic
Imports System.Text
Imports VT105.MinimalisticTelnet

' A very simple Telnet link to the Simh instance.
' This module also translates the keypad codes to the VT100 equivalents
' The exception is '+' which now encodes <esc>OM which simulates the keypad '<keypad>enter' key.
' This is used as the virtual enter key as the actual keypad 'enter' key cannot be distiguished from
' the main 'enter' key.

Module Telnet

    Friend Class Program
        Public Shared Sub Main(ByVal args As String())
            Dim InKey As ConsoleKeyInfo
            Dim CKey As Integer


            InitTerminal()

            While (0)
                System.Threading.Thread.Sleep(20)
                UpdateDisplay()
            End While
            Dim arguments As String() = Environment.GetCommandLineArgs()

            If arguments.Length <> 3 Then
                MsgBox("Usage: VT105 <host> <port>", MsgBoxStyle.Exclamation, "VT105")
                End
            End If

            ' Dim tc As TelnetConnection = New TelnetConnection(arguments(1), arguments(2))
            Dim tc As CommPort = New CommPort("COM12")

            While tc.IsConnected
                TxString(tc.Read())                 ' Follow this function which contains a short delay/timeout
                If Console.KeyAvailable Then
                    InKey = Console.ReadKey(True)
                    CKey = InKey.Key

                    Select Case CKey
                        Case ConsoleKey.NumPad0 To ConsoleKey.NumPad9
                            tc.Write(Chr(27) & "O" & Chr(CKey + Asc("p") - ConsoleKey.NumPad0))
                        Case ConsoleKey.Decimal
                            tc.Write(Chr(27) & "On")
                        Case ConsoleKey.Subtract
                            tc.Write(Chr(27) & "OS")
                        Case ConsoleKey.Multiply
                            tc.Write(Chr(27) & "OR")
                        Case ConsoleKey.Divide
                            tc.Write(Chr(27) & "OQ")
                        Case ConsoleKey.Add
                            tc.Write(Chr(27) & "OM")
                        Case Else
                            If InKey.KeyChar = Chr(8) Then
                                tc.WriteChar(Chr(127))
                            Else
                                tc.WriteChar(InKey.KeyChar)
                            End If
                    End Select

                End If
            End While

            Console.WriteLine("DISCONNECTED by server.....")
            Console.WriteLine("Press enter to exit")
            Console.ReadLine()                      ' 
        End Sub
    End Class

End Module
