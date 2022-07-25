﻿using System;
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

                while (false)
                {
                    System.Threading.Thread.Sleep(20);
                    VT105.UpdateDisplay();
                }
                var arguments = Environment.GetCommandLineArgs();

                //if (arguments.Length != 3)
                //{
                //    Interaction.MsgBox("Usage: VT105 <host> <port>", MsgBoxStyle.Exclamation, "VT105");
                //    Environment.Exit(0);
                //} 

                // Dim tc As TelnetConnection = New TelnetConnection(arguments(1), arguments(2))
                var tc = new CommPort(arguments[1]);

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