using System;
using Microsoft.VisualBasic;
using VT105.MinimalisticTelnet;

namespace VT105
{

    // A very simple Telnet link to the Simh instance.
    // This module also translates the keypad codes to the VT100 equivalents
    // The exception is '+' which now encodes <esc>OM which simulates the keypad '<keypad>enter' key.
    // This is used as the virtual enter key as the actual keypad 'enter' key cannot be distiguished from
    // the main 'enter' key.

    static class Telnet
    {

        internal class Program
        {
            public static void Main(string[] args)
            {
                ConsoleKeyInfo InKey;
                int CKey;

                VT105.InitTerminal();
                var arguments = Environment.GetCommandLineArgs();

                //if (arguments.Length != 3)
                //{
                //    Interaction.MsgBox("Usage: VT105 <host> <port>", MsgBoxStyle.Exclamation, "VT105");
                //    Environment.Exit(0);
                //}

                // Swap these next 2 lines to use a comm port or telnet (to be tidied up)!
                // var tc = new TelnetConnection(arguments[1], int.Parse(arguments[2]));
                var tc = new CommPort(arguments[1]);

                // tc.Login("username", "password", 100);
                // For telnet, you can use this line as well for a totally insecure experiance!
                // NB if you leave this line out, input to the host login/username prompts will not echo.

                // In the CommPort case, arguments[1] is the name of a comm port.


                while (tc.IsConnected)
                {
                    VT105.TxString(tc.Read());                 // Follow this function which contains a short delay/timeout
                    if (Console.KeyAvailable)
                    {
                        InKey = Console.ReadKey(true);
                        CKey = (int)InKey.Key;

                        switch (CKey)
                        {
                            case var @case when (int)ConsoleKey.NumPad0 <= @case && @case <= (int)ConsoleKey.NumPad9:
                                {
                                    tc.Write('\u001b' + "O" + Strings.Chr(CKey + Strings.Asc("p") - (int)ConsoleKey.NumPad0));
                                    break;
                                }
                            case (int)ConsoleKey.Decimal:
                                {
                                    tc.Write('\u001b' + "On");
                                    break;
                                }
                            case (int)ConsoleKey.Subtract:
                                {
                                    tc.Write('\u001b' + "OS");
                                    break;
                                }
                            case (int)ConsoleKey.Multiply:
                                {
                                    tc.Write('\u001b' + "OR");
                                    break;
                                }
                            case (int)ConsoleKey.Divide:
                                {
                                    tc.Write('\u001b' + "OQ");
                                    break;
                                }
                            case (int)ConsoleKey.Add:
                                {
                                    tc.Write('\u001b' + "OM");
                                    break;
                                }

                            default:
                                {
                                    if (InKey.KeyChar == '\b')
                                    {
                                        tc.WriteChar('\u007f');
                                    }
                                    else
                                    {
                                        tc.WriteChar(InKey.KeyChar);
                                    }

                                    break;
                                }
                        }

                    }
                }

                Console.WriteLine("DISCONNECTED by server.....");
                Console.WriteLine("Press enter to exit");
                Console.ReadLine();                      // 
            }
        }

    }
}