using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace VT105
{



    static class VT105
    {
        // For SetConsoleMode (input)
        private const int ENABLE_LINE_INPUT = 0x2;
        private const int ENABLE_ECHO_INPUT = 0x4;
        private const int ENABLE_MOUSE_INPUT = 0x10;
        private const int ENABLE_PROCESSED_INPUT = 0x1;
        private const int ENABLE_WINDOW_INPUT = 0x8;
        // For SetConsoleMode (output)
        private const int ENABLE_PROCESSED_OUTPUT = 0x1;
        private const int ENABLE_WRAP_AT_EOL_OUTPUT = 0x2;
        private const int ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x4;
        private const int ENABLE_VIRTUAL_TERMINAL_INPUT = 0x200;
        private const int STD_OUTPUT_HANDLE = -11;
        private const int STD_INPUT_HANDLE = -10;
        private const int SRCPAINT = 0xEE0086;
        private const int MERGECOPY = 0xC000CA;
        private const int MERGEPAINT = 0xBB0226;
        private const int SRCCOPY = 0xCC0020;
        private const int Lmargin = 105;         // Centre of char 8 with font @ 14px wide.
        private const int Rmargin = 107;          // Centre of char 74 from Right margin of screen
        private const double XScale = 1.8d;

        [DllImport("kernel32")]
        static extern int SetConsoleMode(long hConsoleHandle, long dwMode);
        [DllImport("kernel32")]
        static extern int GetNumberOfConsoleFonts();
        [DllImport("kernel32")]
        static extern long GetStdHandle(long nStdHandle);
        [DllImport("kernel32.dll")]
        static extern int SetConsoleFont(IntPtr hOut, uint dwFontSize);
        [DllImport("user32.dll")]
        static extern int GetDC(int hwnd);
        [DllImport("user32.dll")]
        static extern int ReleaseDC(int hwnd, int hdc);
        [DllImport("gdi32.dll")]
        static extern int SetPixel(int hdc, int x, int y, int crColor);
        [DllImport("gdi32.dll")]
        static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);
        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        private static int[] InBuff = new int[9];
        private static int InCnt;
        private static int Ocnt;
        private static IntPtr Hwnd;
        private static bool GMode;
        private static int ArgCnt;
        private static int[] ArgBuf = new int[5];
        private static int Arg;
        private static int Ndx;
        private static Bitmap Bmp;
        private static Graphics Gr;
        private static Graphics GrWin;
        private static IntPtr HBmp;
        private static int VT_Cr0;
        private static int VT_CR1;
        private static int VT_Xbase;
        private static int Hline;
        private static int Vline;
        private static int Bline0;
        private static int Bline1;
        private static Pen Wpen;
        private static Pen DashPen;
        private static Pen BlPen;
        private static int[] GFlg0 = new int[513];
        private static int[] GFlg1 = new int[513];
        private static int[] VFlg = new int[513];
        private static int Updt;

        [StructLayout(LayoutKind.Sequential)]
        internal struct COORD
        {
            internal short X;
            internal short Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CONSOLE_FONT_INFO_EX
        {
            internal int cbSize;
            internal int nFont;
            internal COORD dwFontSize;
            internal int FontFamily;
            internal int FontWeight;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] FaceName;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool GetCurrentConsoleFontEx(IntPtr consoleOutput, bool maximumWindow, ref CONSOLE_FONT_INFO_EX lpConsoleCurrentFontEx);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetCurrentConsoleFontEx(IntPtr consoleOutput, bool maximumWindow, CONSOLE_FONT_INFO_EX consoleCurrentFontEx);
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr GetConsoleWindow();

        public static void InitTerminal()
        {

            long Chndl;
            long Mhndl;
            var FaceName = new byte[85];
            string FName;
            byte[] Fb;

            Chndl = (long)GetStdHandle(STD_OUTPUT_HANDLE);
            Mhndl = (long)GetStdHandle(STD_INPUT_HANDLE);
            SetConsoleMode(Chndl, ENABLE_PROCESSED_OUTPUT | ENABLE_VIRTUAL_TERMINAL_PROCESSING);
            SetConsoleMode(Mhndl, ENABLE_VIRTUAL_TERMINAL_INPUT);
            // Instantiating CONSOLE_FONT_INFO_EX and setting its size (the function will fail otherwise)
            var ConsoleFontInfo = new CONSOLE_FONT_INFO_EX();
            ConsoleFontInfo.cbSize = Marshal.SizeOf(ConsoleFontInfo);
            // Optional, implementing this will keep the fontweight and fontsize from changing
            GetCurrentConsoleFontEx((IntPtr)Chndl, false, ref ConsoleFontInfo);
            FName = System.Text.Encoding.Unicode.GetString(ConsoleFontInfo.FaceName).Trim();
            Fb = System.Text.Encoding.ASCII.GetBytes("Courier New");
            Array.Resize(ref Fb, ConsoleFontInfo.FaceName.Length);
            // ConsoleFontInfo.FaceName = Fb
            ConsoleFontInfo.dwFontSize.X = 14;
            ConsoleFontInfo.dwFontSize.Y = 24;
            SetCurrentConsoleFontEx((IntPtr)Chndl, false, ConsoleFontInfo);
            Console.WindowWidth = 80;
            Console.WindowHeight = 24;
            Console.SetBufferSize(80, 24);
            Console.TreatControlCAsInput = true;
            InCnt = 0;
            Ocnt = 0;
            GMode = false;
            Hwnd = GetConsoleWindow();
            GrWin = Graphics.FromHwnd(Hwnd);
            InitGraphics();
            Ndx = 0;
            // Console.Write(Chr(27) & "=")
            Console.Write('\u001b' + "[?1h");

        }

        private static void InitGraphics()
        {
            IntPtr hDc;

            hDc = (IntPtr)GetDC((int)Hwnd);
            HBmp = CreateCompatibleBitmap(hDc, (int)Math.Round(GrWin.VisibleClipBounds.Width), (int)Math.Round(GrWin.VisibleClipBounds.Height));
            ReleaseDC((int)Hwnd, (int)hDc);
            Bmp = Image.FromHbitmap(HBmp);
            Bmp.MakeTransparent();
            Gr = Graphics.FromImage(Bmp);
            Wpen = new Pen(Color.White, 2f);
            DashPen = new Pen(Color.White, 2f);
            BlPen = new Pen(Color.Black, 2f);
            DashPen.DashPattern = new float[] { 2.0f, 2.0f };
            Array.Clear(GFlg0, 0, 512);
            Array.Clear(GFlg1, 0, 512);
            Array.Clear(VFlg, 0, 512);
            Bline0 = 0;
            Bline1 = 0;
            Updt = 0;

        }

        public static void TxString(string Str)
        {
            int I;
            int Tm;

            UpdateDisplay();

            if (Strings.Len(Str) == 0)
                return;

            var loopTo = Strings.Len(Str) - 1;
            for (I = 0; I <= loopTo; I++)
            {
                Tm = Strings.Asc(Str[I]);
                if (Tm > 0)
                    SendChar(Tm);
            }
        }

        public static void UpdateDisplay()
        {
            Updt += 1;
            if (Updt > 20)
            {
                GrWin.DrawImage(Bmp, 0, 0);
                Updt = 0;
            }


        }

        private static object GetRchar()
        {
            object GetRcharRet = default;

            GetRcharRet = InBuff[Ocnt];
            Ocnt = Ocnt + 1 & 7;
            return GetRcharRet;

        }

        private static void SetRchar(int Val)
        {

            InBuff[InCnt] = Val;
            InCnt = InCnt + 1 & 7;

        }

        private static int RDiff()
        {

            return InCnt - Ocnt & 7;

        }

        private static void DcdGraphic(int Val)
        {

            int Yval;
            int Xval;

            // Debug.Print(Val.ToString() + ":" + Strings.Chr(Val));
            Updt = 18;
            if (ArgCnt > 0)
            {
                ArgBuf[ArgCnt] = Val;
                ArgCnt -= 1;
                if ((Val & 32) == 0)
                {
                    ArgCnt -= 1;
                }
                if (ArgCnt > 0)
                    return;
            }

            if (ArgCnt == 0)
            {
                switch (Strings.Chr(Arg))
                {
                    case 'A':
                        {
                            VT_Cr0 = ArgBuf[2] | ArgBuf[1] << 8;
                            if ((VT_Cr0 & 7) == 0)
                            {
                                Gr.Clear(Color.Transparent);
                            }

                            break;
                        }
                    case 'I':
                        {
                            VT_CR1 = ArgBuf[2] | ArgBuf[1] << 8;
                            Val = ArgBuf[2] & 16;
                            if (Conversions.ToBoolean(Val))
                            {
                                System.Threading.Thread.Sleep(100);
                                Gr.Dispose();
                                Bmp.Dispose();
                                InitGraphics();
                            }

                            break;
                        }
                    case 'D':
                        {
                            Hline = ArgBuf[2] & 31 | (ArgBuf[1] & 7) << 5;
                            Val = ArgBuf[1] & 16;
                            if (Conversions.ToBoolean(Val))
                            {
                                Gr.DrawLine(Wpen, Lmargin, (int)Math.Round((240 - Hline) * 2.4d), Bmp.Width - Rmargin, (int)Math.Round((240 - Hline) * 2.4d));
                            }

                            break;
                        }
                    case 'L':
                        {
                            Vline = ArgBuf[2] & 31 | (ArgBuf[1] & 15) << 5;
                            Val = ArgBuf[1] & 16;
                            if (Conversions.ToBoolean(Val))
                            {
                                Gr.DrawLine(Wpen, (int)Math.Round(Vline * XScale) + Lmargin, 0, (int)Math.Round(Vline * XScale) + Lmargin, (int)Math.Round((240 - 51) * 2.4d));
                                VFlg[Vline] = 1;
                            }

                            break;
                        }
                    case '@':
                        {
                            if (Conversions.ToBoolean(VT_Cr0 & 256))
                            {
                                Bline1 = ArgBuf[2] & 31 | (ArgBuf[1] & 7) << 5;
                                Bline1 = (int)Math.Round((240 - Bline1) * 2.4d);
                            }
                            else
                            {
                                Bline0 = ArgBuf[2] & 31 | (ArgBuf[1] & 7) << 5;
                                Bline0 = (int)Math.Round((240 - Bline0) * 2.4d);
                            }

                            break;
                        }
                    case 'H':
                        {
                            VT_Xbase = ArgBuf[2] & 31 | (ArgBuf[1] & 15) << 5;
                            break;
                        }
                    case 'B':
                        {
                            Val = ArgBuf[2] & 31 | (ArgBuf[1] & 7) << 5;
                            Yval = (int)Math.Round((240 - Val) * 2.4d);
                            Xval = (int)Math.Round(VT_Xbase * XScale) + Lmargin;
                            if (Conversions.ToBoolean(VT_Cr0 & 512))
                            {
                                Yval = Bline0;
                            }
                            if ((VT_Cr0 & 2) > 0)
                            {
                                if (VT_Xbase > 0)
                                {
                                    Gr.DrawLine(Wpen, Xval, (int)Math.Round((240 - Val) * 2.4d) + 1, Xval, Yval);
                                }
                            }
                            else
                            {
                                Gr.DrawLine(BlPen, Xval, (int)Math.Round((240 - Val) * 2.4d) + 1, Xval, Yval);
                            }
                            GFlg0[VT_Xbase] = Val;
                            VT_Xbase = VT_Xbase + 1 & 511;
                            break;
                        }
                    case 'J':
                        {
                            Val = ArgBuf[2] & 31 | (ArgBuf[1] & 7) << 5;
                            Yval = (int)Math.Round((240 - Val) * 2.4d);
                            Xval = (int)Math.Round(VT_Xbase * XScale) + Lmargin;
                            if (Conversions.ToBoolean(VT_Cr0 & 512))
                            {
                                Yval = Bline1;
                            }
                            if (Conversions.ToBoolean(VT_Cr0 & 2))
                            {
                                if (VT_Xbase > 0)
                                {
                                    Gr.DrawLine(Wpen, Xval, (int)Math.Round((240 - Val) * 2.4d) + 1, Xval, Yval);
                                }
                            }
                            GFlg1[VT_Xbase] = Val;
                            VT_Xbase = VT_Xbase + 1 & 511;
                            break;
                        }
                    case 'C':
                        {
                            Val = ArgBuf[2] & 31 | (ArgBuf[1] & 15) << 5;
                            if ((ArgBuf[1] & 16) > 0)
                            {
                                Xval = (int)Math.Round(Val * XScale) + Lmargin;
                                Yval = (int)Math.Round((240 - GFlg0[Val]) * 2.4d);
                                // If Val > 0 Then
                                // If VFlg(Val) > 0 Then
                                // Gr.DrawLine(BlPen, Xval, 0, Xval, Yval)
                                // Gr.DrawLine(DashPen, Xval, 0, Xval, Yval)
                                // Else
                                // Gr.DrawLine(BlPen, Xval, 0, Xval, Yval)
                                // End If
                                // End If
                                Gr.DrawLine(Wpen, Xval, Yval - 5, Xval, Yval + 10);
                            }

                            break;
                        }
                    case 'K':
                        {
                            Val = ArgBuf[2] & 31 | (ArgBuf[1] & 15) << 5;
                            if ((ArgBuf[1] & 16) > 0)
                            {
                                Xval = (int)Math.Round(Val * XScale) + Lmargin;
                                Yval = (int)Math.Round((240 - GFlg1[Val]) * 2.4d);
                                // If Val > 0 Then
                                // If VFlg(Val) > 0 Then
                                // Gr.DrawLine(BlPen, Xval, 0, Xval, Yval)
                                // Gr.DrawLine(DashPen, Xval, 0, Xval, Yval)
                                // Else
                                // Gr.DrawLine(BlPen, Xval, 0, Xval, Yval)
                                // End If
                                // End If
                                Gr.DrawLine(Wpen, Xval, Yval - 5, Xval, Yval + 10);
                            }

                            break;
                        }
                }
                ArgCnt = -1;
                return;
            }

            switch (Strings.Chr(Val))
            {
                case 'A':
                case 'H':
                case 'B':
                case 'J':
                case '@':
                case 'C':
                case 'L':
                case 'I':
                case 'K':
                    {
                        ArgCnt = 2;
                        Arg = Val;
                        break;
                    }
                case 'D':
                    {
                        ArgCnt = 2;
                        Arg = Val;
                        break;
                    }

                default:
                    {
                        switch (Strings.Chr(Arg))
                        {
                            case 'B':
                            case 'J':
                            case 'C':
                            case 'L':
                            case 'D':
                            case 'K':
                            case '@':
                                {
                                    ArgBuf[2] = Val;
                                    ArgCnt = 1;
                                    break;
                                }

                            default:
                                {
                                    Console.WriteLine("Graphics sequence error ... any key to exit.");
                                    Console.ReadKey();
                                    Environment.Exit(0);
                                    break;
                                }
                        }

                        break;
                    }
            }

        }


        private static void SendChar(int Chrx)
        {


            if (RDiff() == 1 & Chrx == Strings.Asc("1"))
            {
                GMode = true;
                GetRchar();
                ArgCnt = -1;
                return;
            }

            if (RDiff() == 1 & Chrx == Strings.Asc("2"))
            {
                GMode = false;
                GetRchar();
                return;
            }

            if (Chrx == 27)
            {
                SetRchar(Chrx);
                return;
            }

            if (RDiff() == 1)
            {
                Console.Write(Strings.Chr(Conversions.ToInteger(GetRchar())));
            }

            if (GMode)
            {
                DcdGraphic(Chrx);
                return;
            }
            else
            {
                 Console.Write(Strings.Chr(Chrx));
            }


        }

    }
}