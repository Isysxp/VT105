
using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualBasic;

// This module is based upon Jon Sagara's rebuild of Tom Janssen's Minimalistic Telnet. See: https://github.com/jonsagara/MinimalisticTelnet
// Site: https://github.com/jonsagara



namespace VT105.MinimalisticTelnet
{
    internal enum Verbs
    {
        WILL = 251,
        WONT = 252,
        DO = 253,
        DONT = 254,
        IAC = 255
    }

    internal enum Options
    {
        SGA = 3
    }

    internal class TelnetConnection
    {
        private TcpClient tcpSocket;
        private int TimeOutMs = 10;

        public void NoKeep(TcpClient tcl)
        {
            const uint on = 0;
            const uint time = 2000;
            const uint interval = 2000;

            byte[] inOptionValues = new byte[Marshal.SizeOf(on) * 3];

            BitConverter.GetBytes(on).CopyTo(inOptionValues, 0);
            BitConverter.GetBytes(time).CopyTo(inOptionValues, Marshal.SizeOf(on));
            BitConverter.GetBytes(interval).CopyTo(inOptionValues, Marshal.SizeOf(on) * 2);

            tcl.Client.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);
        }
        public TelnetConnection(string Hostname, int Port)
        {
            tcpSocket = new TcpClient();
            //Socket s = tcpSocket.Client;
            //s.SetSocketOption(SocketOptionLevel.Socket , SocketOptionName.KeepAlive, false);
            NoKeep(tcpSocket);
            tcpSocket.Connect(Hostname, Port);
        }

        public string Login(string Username, string Password, int LoginTimeOutMs)
        {
            int oldTimeOutMs = TimeOutMs;
            string s="";
            TimeOutMs = LoginTimeOutMs;
            s = Read();
            if (!s.TrimEnd().EndsWith(":"))
                throw new Exception("Failed to connect : no login prompt");
            WriteLine(Username);
            s += Read();
            if (!s.TrimEnd().EndsWith(":"))
                throw new Exception("Failed to connect : no password prompt");
            WriteLine(Password);
            s += Read();
            TimeOutMs = oldTimeOutMs;
            return s;
        }

        public void WriteLine(string cmd)
        {
            Write(cmd + Constants.vbLf);
        }

        public void Write(string cmd)
        {
            if (!tcpSocket.Connected)
                return;
            var buf = Encoding.ASCII.GetBytes(cmd.Replace(Constants.vbNullChar + "xFF", Constants.vbNullChar + "xFF" + Constants.vbNullChar + "xFF"));
            tcpSocket.GetStream().Write(buf, 0, buf.Length);
        }

        public void WriteChar(char Chr)
        {
            if (!tcpSocket.Connected)
                return;
            var buf = Encoding.ASCII.GetBytes(Chr.ToString());
            tcpSocket.GetStream().Write(buf, 0, buf.Length);
        }

        public string Read()
        {
            if (!tcpSocket.Connected)
                return null;
            var sb = new StringBuilder();

            do
            {
                ParseTelnet(sb);
                System.Threading.Thread.Sleep(TimeOutMs);
                VT105.UpdateDisplay();                             // Added by ISS. Update console window every 0.01 Sec as the console handler also updates this window in realtime.
            }
            while (tcpSocket.Available > 0);

            return sb.ToString();
        }

        public bool IsConnected
        {
            get
            {
                return tcpSocket.Connected;
            }
        }

        void ParseTelnet(StringBuilder sb)
        {
            while (tcpSocket.Available > 0)
            {
                int input = tcpSocket.GetStream().ReadByte();
                switch (input)
                {
                    case -1:
                        break;
                    case (int)Verbs.IAC:
                        // interpret as command
                        int inputverb = tcpSocket.GetStream().ReadByte();
                        if (inputverb == -1) break;
                        switch (inputverb)
                        {
                            case (int)Verbs.IAC:
                                //literal IAC = 255 escaped, so append char 255 to string
                                sb.Append(inputverb);
                                break;
                            case (int)Verbs.DO:
                            case (int)Verbs.DONT:
                            case (int)Verbs.WILL:
                            case (int)Verbs.WONT:
                                // reply to all commands with "WONT", unless it is SGA (suppres go ahead)
                                int inputoption = tcpSocket.GetStream().ReadByte();
                                if (inputoption == -1) break;
                                tcpSocket.GetStream().WriteByte((byte)Verbs.IAC);
                                if (inputoption == (int)Options.SGA)
                                    tcpSocket.GetStream().WriteByte(inputverb == (int)Verbs.DO ? (byte)Verbs.WILL : (byte)Verbs.DO);
                                else
                                    tcpSocket.GetStream().WriteByte(inputverb == (int)Verbs.DO ? (byte)Verbs.WONT : (byte)Verbs.DONT);
                                tcpSocket.GetStream().WriteByte((byte)inputoption);
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        sb.Append((char)input);
                        break;
                }
            }
        }
    }
}