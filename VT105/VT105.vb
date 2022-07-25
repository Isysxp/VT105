Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Drawing



Module VT105
    'For SetConsoleMode (input)
    Private Const ENABLE_LINE_INPUT = &H2
    Private Const ENABLE_ECHO_INPUT = &H4
    Private Const ENABLE_MOUSE_INPUT = &H10
    Private Const ENABLE_PROCESSED_INPUT = &H1
    Private Const ENABLE_WINDOW_INPUT = &H8
    'For SetConsoleMode (output)
    Private Const ENABLE_PROCESSED_OUTPUT = &H1
    Private Const ENABLE_WRAP_AT_EOL_OUTPUT = &H2
    Private Const ENABLE_VIRTUAL_TERMINAL_PROCESSING = &H4
    Private Const ENABLE_VIRTUAL_TERMINAL_INPUT = &H200
    Private Const STD_OUTPUT_HANDLE = -11
    Private Const STD_INPUT_HANDLE = -10
    Private Const SRCPAINT = &HEE0086
    Private Const MERGECOPY = &HC000CA
    Private Const MERGEPAINT = &HBB0226
    Private Const SRCCOPY = &HCC0020
    Private Const Lmargin = 105         ' Centre of char 8 with font @ 14px wide.
    Private Const Rmargin = 75          ' Centre of char 74 from Right margin of screen
    Private Const XScale = 1.8

    Declare Function SetConsoleMode Lib "kernel32" (ByVal hConsoleHandle As Long, ByVal dwMode As Long) As Integer
    Declare Function GetNumberOfConsoleFonts Lib "kernel32" () As Integer
    Declare Function GetStdHandle Lib "kernel32" (ByVal nStdHandle As Long) As Long
    Declare Function SetConsoleFont Lib "kernel32.dll" (ByVal hOut As IntPtr, ByVal dwFontSize As UInt32) As Integer
    Declare Function GetDC Lib "user32.dll" (ByVal hwnd As Int32) As Int32
    Declare Function ReleaseDC Lib "user32.dll" (ByVal hwnd As Int32, ByVal hdc As Int32) As Int32
    Declare Function SetPixel Lib "gdi32.dll" (ByVal hdc As Integer, ByVal x As Integer, ByVal y As Integer, ByVal crColor As Integer) As Integer
    Declare Function BitBlt Lib "gdi32.dll" (ByVal hdcDest As IntPtr, ByVal nXDest As Integer, ByVal nYDest As Integer, ByVal nWidth As Integer, ByVal nHeight As Integer, ByVal hdcSrc As IntPtr, ByVal nXSrc As Integer, ByVal nYSrc As Integer, ByVal dwRop As Int32) As Boolean
    Declare Function CreateCompatibleBitmap Lib "gdi32.dll" (ByVal hdc As IntPtr, ByVal nWidth As Integer, ByVal nHeight As Integer) As IntPtr
    Declare Function CreateCompatibleDC Lib "gdi32.dll" (ByVal hdc As IntPtr) As IntPtr

    Dim InBuff(8) As Integer, InCnt As Integer, Ocnt As Integer
    Dim Hwnd As IntPtr
    Dim GMode As Boolean, ArgCnt As Integer, ArgBuf(4) As Integer, Arg As Integer
    Dim Bmp As Bitmap, Gr As Graphics, GrWin As Graphics, HBmp As IntPtr, Ndx As Integer, BmpDC As IntPtr
    Dim VT_Cr0 As Integer, VT_CR1 As Integer, VT_Xbase As Integer, Hline As Integer, Vline As Integer, Bline0 As Integer, Bline1 As Integer
    Dim Wpen As Pen, DashPen As Pen, BlPen As Pen
    Dim GFlg0(512) As Integer, GFlg1(512) As Integer, VFlg(512) As Integer, Updt As Integer

    <StructLayout(LayoutKind.Sequential)> _
Friend Structure COORD
        Friend X As Short
        Friend Y As Short
    End Structure

    <StructLayout(LayoutKind.Sequential)> _
    Friend Structure CONSOLE_FONT_INFO_EX
        Friend cbSize As Int32
        Friend nFont As Int32
        Friend dwFontSize As COORD
        Friend FontFamily As Int32
        Friend FontWeight As Int32
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=64)> _
        Public FaceName() As Byte
    End Structure

    <DllImport("kernel32.dll", SetLastError:=True)> _
     Private Function GetStdHandle(ByVal nStdHandle As Integer) As IntPtr
    End Function

    <DllImport("kernel32.dll", CharSet:=CharSet.Unicode, SetLastError:=True)> _
    Private Function GetCurrentConsoleFontEx(ByVal consoleOutput As IntPtr, ByVal maximumWindow As Boolean, ByRef lpConsoleCurrentFontEx As CONSOLE_FONT_INFO_EX) As Boolean
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)> _
    Private Function SetCurrentConsoleFontEx(ByVal consoleOutput As IntPtr, ByVal maximumWindow As Boolean, ByVal consoleCurrentFontEx As CONSOLE_FONT_INFO_EX) As Boolean
    End Function
    <DllImport("kernel32.dll", SetLastError:=True, CharSet:=CharSet.Auto)> _
    Private Function GetConsoleWindow() As IntPtr
    End Function

    Sub InitTerminal()

        'Dim Istr As Stream = File.Open("teraterm.log", FileMode.Open)
        'Dim Inf As New BinaryReader(Istr)
        'Dim Ich As Integer, Fcnt As Long
        Dim Chndl As Long, Mhndl As Long
        Dim FaceName(84) As Byte, FName As String
        Dim Fb() As Byte

        Chndl = GetStdHandle(STD_OUTPUT_HANDLE)
        Mhndl = GetStdHandle(STD_INPUT_HANDLE)
        SetConsoleMode(Chndl, ENABLE_PROCESSED_OUTPUT Or ENABLE_VIRTUAL_TERMINAL_PROCESSING)
        SetConsoleMode(Mhndl, ENABLE_VIRTUAL_TERMINAL_INPUT)
        ' Instantiating CONSOLE_FONT_INFO_EX and setting its size (the function will fail otherwise)
        Dim ConsoleFontInfo = New CONSOLE_FONT_INFO_EX()
        ConsoleFontInfo.cbSize = Marshal.SizeOf(ConsoleFontInfo)
        ' Optional, implementing this will keep the fontweight and fontsize from changing
        GetCurrentConsoleFontEx(Chndl, False, ConsoleFontInfo)
        FName = System.Text.Encoding.Unicode.GetString(ConsoleFontInfo.FaceName).Trim
        Fb = System.Text.Encoding.ASCII.GetBytes("Courier New")
        ReDim Preserve Fb(ConsoleFontInfo.FaceName.Length - 1)
        'ConsoleFontInfo.FaceName = Fb
        ConsoleFontInfo.dwFontSize.X = 14
        ConsoleFontInfo.dwFontSize.Y = 24
        SetCurrentConsoleFontEx(Chndl, False, ConsoleFontInfo)
        Console.WindowWidth = 80
        Console.WindowHeight = 24
        Console.SetBufferSize(80, 24)
        Console.TreatControlCAsInput = True
        InCnt = 0
        Ocnt = 0
        GMode = False
        Hwnd = GetConsoleWindow()
        GrWin = Graphics.FromHwnd(Hwnd)
        InitGraphics()
        Ndx = 0
        'Console.Write(Chr(27) & "=")
        Console.Write(Chr(27) & "[?1h")
        'While Inf.BaseStream.Position <> Inf.BaseStream.Length
        '    Ich = Inf.ReadByte()
        '    'SendChar(Ich)
        '    Ndx += 1
        'End While
        'Inf.Close()


    End Sub

    Private Sub InitGraphics()
        Dim hDc As IntPtr

        hDc = GetDC(Hwnd)
        HBmp = CreateCompatibleBitmap(hDc, GrWin.VisibleClipBounds.Width, GrWin.VisibleClipBounds.Height)
        ReleaseDC(Hwnd, hDc)
        Bmp = Image.FromHbitmap(HBmp)
        Bmp.MakeTransparent()
        Gr = Graphics.FromImage(Bmp)
        Wpen = New Pen(Color.White, 2)
        DashPen = New Pen(Color.White, 2)
        BlPen = New Pen(Color.Black, 2)
        DashPen.DashPattern = New Single() {2.0, 2.0}
        Array.Clear(GFlg0, 0, 512)
        Array.Clear(GFlg1, 0, 512)
        Array.Clear(VFlg, 0, 512)
        Bline0 = 0
        Bline1 = 0
        Updt = 0

    End Sub

    Public Sub TxString(ByVal Str As String)
        Dim I As Integer, Tm As Integer

        If Len(Str) = 0 Then Return

        For I = 0 To Len(Str) - 1
            Tm = Asc(Str(I))
            If Tm > 0 Then SendChar(Tm)
        Next

    End Sub

    Public Sub UpdateDisplay()

        GrWin.DrawImage(Bmp, 0, 0)              ' Add our graphics to the console window, bmp background is transparent so the console data is still visible.

    End Sub

    Private Function GetRchar()

        GetRchar = InBuff(Ocnt)
        Ocnt = (Ocnt + 1) And 7

    End Function

    Private Sub SetRchar(ByVal Val As Integer)

        InBuff(InCnt) = Val
        InCnt = (InCnt + 1) And 7

    End Sub

    Private Function RDiff() As Integer

        Return (InCnt - Ocnt) And 7

    End Function

    Private Sub DcdGraphic(ByVal Val As Integer)

        Dim Yval As Integer, Xval As Integer

        Debug.Print(Val.ToString & ":" & Chr(Val))
        Updt += 1
        If Updt = 100 Then
            UpdateDisplay()
            Updt = 0
        End If

        If ArgCnt > 0 Then
            ArgBuf(ArgCnt) = Val
            ArgCnt -= 1
            If (Val And 32) = 0 Then
                ArgCnt -= 1
            End If
            If ArgCnt > 0 Then Return
        End If

        If ArgCnt = 0 Then
            Select Case Chr(Arg)
                Case "A"
                    VT_Cr0 = ArgBuf(2) Or (ArgBuf(1) << 8)
                    If (VT_Cr0 And 7) = 0 Then
                        Gr.Clear(Color.Transparent)
                    End If
                Case "I"
                    VT_CR1 = ArgBuf(2) Or (ArgBuf(1) << 8)
                    Val = ArgBuf(2) And 16
                    If Val Then
                        Gr.Dispose()
                        Bmp.Dispose()
                        InitGraphics()
                    End If
                Case "D"
                    Hline = (ArgBuf(2) And 31) Or ((ArgBuf(1) And 7) << 5)
                    Val = ArgBuf(1) And 16
                    If (Val) Then
                        Gr.DrawLine(Wpen, Lmargin, CInt((240 - Hline) * 2.4), Bmp.Width - Rmargin, CInt((240 - Hline) * 2.4))
                    End If
                Case "L"
                    Vline = (ArgBuf(2) And 31) Or ((ArgBuf(1) And 15) << 5)
                    Val = ArgBuf(1) And 16
                    If (Val) Then
                        Gr.DrawLine(Wpen, CInt(Vline * XScale) + Lmargin, 0, CInt(Vline * XScale) + Lmargin, CInt((240 - 51) * 2.4))
                        VFlg(Vline) = 1
                    End If
                Case "@"
                    If VT_Cr0 And 256 Then
                        Bline1 = (ArgBuf(2) And 31) Or ((ArgBuf(1) And 7) << 5)
                        Bline1 = CInt((240 - Bline1) * 2.4)
                    Else
                        Bline0 = (ArgBuf(2) And 31) Or ((ArgBuf(1) And 7) << 5)
                        Bline0 = CInt((240 - Bline0) * 2.4)
                    End If
                Case "H"
                    VT_Xbase = (ArgBuf(2) And 31) Or ((ArgBuf(1) And 15) << 5)
                Case "B"
                    Val = (ArgBuf(2) And 31) Or ((ArgBuf(1) And 7) << 5)
                    Yval = CInt((240 - Val) * 2.4)
                    Xval = CInt(VT_Xbase * XScale) + Lmargin
                    If VT_Cr0 And 512 Then
                        Yval = Bline0
                    End If
                    If (VT_Cr0 And 2) > 0 Then
                        If (VT_Xbase > 0) Then
                            Gr.DrawLine(Wpen, Xval, CInt((240 - Val) * 2.4) + 1, Xval, Yval)
                        End If
                    Else
                        Gr.DrawLine(BlPen, Xval, CInt((240 - Val) * 2.4) + 1, Xval, Yval)
                    End If
                    GFlg0(VT_Xbase) = Val
                    VT_Xbase = (VT_Xbase + 1) And 511
                Case "J"
                    Val = (ArgBuf(2) And 31) Or ((ArgBuf(1) And 7) << 5)
                    Yval = CInt((240 - Val) * 2.4)
                    Xval = CInt(VT_Xbase * XScale) + Lmargin
                    If VT_Cr0 And 512 Then
                        Yval = Bline1
                    End If
                    If VT_Cr0 And 2 Then
                        If (VT_Xbase > 0) Then
                            Gr.DrawLine(Wpen, Xval, CInt((240 - Val) * 2.4) + 1, Xval, Yval)
                        End If
                    End If
                    GFlg1(VT_Xbase) = Val
                    VT_Xbase = (VT_Xbase + 1) And 511
                Case "C"
                    Val = (ArgBuf(2) And 31) Or ((ArgBuf(1) And 15) << 5)
                    If (ArgBuf(1) And 16) > 0 Then
                        Xval = CInt(Val * XScale) + Lmargin
                        Yval = CInt((240 - GFlg0(Val)) * 2.4)
                        'If Val > 0 Then
                        '    If VFlg(Val) > 0 Then
                        '        Gr.DrawLine(BlPen, Xval, 0, Xval, Yval)
                        '        Gr.DrawLine(DashPen, Xval, 0, Xval, Yval)
                        '    Else
                        '        Gr.DrawLine(BlPen, Xval, 0, Xval, Yval)
                        '    End If
                        'End If
                        Gr.DrawLine(Wpen, Xval, Yval - 5, Xval, Yval + 10)
                    End If
                Case "K"
                    Val = (ArgBuf(2) And 31) Or ((ArgBuf(1) And 15) << 5)
                    If (ArgBuf(1) And 16) > 0 Then
                        Xval = CInt(Val * XScale) + Lmargin
                        Yval = CInt((240 - GFlg1(Val)) * 2.4)
                        '    If Val > 0 Then
                        '        If VFlg(Val) > 0 Then
                        '            Gr.DrawLine(BlPen, Xval, 0, Xval, Yval)
                        '            Gr.DrawLine(DashPen, Xval, 0, Xval, Yval)
                        '        Else
                        '            Gr.DrawLine(BlPen, Xval, 0, Xval, Yval)
                        '        End If
                        '   End If
                        Gr.DrawLine(Wpen, Xval, Yval - 5, Xval, Yval + 10)
                    End If
            End Select
            ArgCnt = -1
            Return
        End If

        Select Case Chr(Val)
            Case "A", "H", "B", "J", "@", "C", "L", "I", "K"
                ArgCnt = 2
                Arg = Val
            Case "D"
                ArgCnt = 2
                Arg = Val
            Case Else
                Select Case Chr(Arg)
                    Case "B", "J", "C", "L", "D", "K", "@"
                        ArgBuf(2) = Val
                        ArgCnt = 1
                    Case Else
                        Console.WriteLine("Graphics sequence error ... any key to exit.")
                        Console.ReadKey()
                        End
                End Select
        End Select

    End Sub


    Private Sub SendChar(ByVal Chrx As Integer)


        If RDiff() = 1 And Chrx = Asc("1") Then
            GMode = True
            GetRchar()
            ArgCnt = -1
            Return
        End If

        If RDiff() = 1 And Chrx = Asc("2") Then
            GMode = False
            GetRchar()
            Return
        End If

        If Chrx = 27 Then
            SetRchar(Chrx)
            Return
        End If

        If RDiff() = 1 Then
            Console.Write(Chr(GetRchar()))
        End If

        If GMode Then
            DcdGraphic(Chrx)
            Return
        Else
            Console.Write(Chr(Chrx))
        End If


    End Sub

End Module
