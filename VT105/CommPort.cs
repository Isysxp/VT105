using System;
using System.IO.Ports;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace VT105
{


    public class CommPort
    {
        private SerialPort Cport;
        private int Dsync;


        public CommPort(string Port)
        {
            Cport = new SerialPort(Port);
            Cport.BaudRate = 115200;
            Cport.DtrEnable = true;
            Cport.RtsEnable = true;
            Cport.Open();
            Dsync = 0;
        }

        public void WriteLine(string cmd)
        {
            Cport.Write(cmd + Constants.vbCrLf);
        }

        public void Write(string cmd)
        {
            Cport.Write(cmd);
        }

        public void WriteChar(char Chr)
        {
            Cport.Write(Conversions.ToString(Chr));
        }

        public string Read()
        {
            if (!Cport.IsOpen)
            {
                return null;
            }

            // Dim sb As StringBuilder = New StringBuilder()
            string sb = "";
            string Bfr;
            try
            {
                if (Cport.BytesToRead > 0)
                {
                    Bfr = Cport.ReadExisting();
                    // Dim Bfr(Cport.BytesToRead + 10) As Byte
                    // Dtr = Cport.Read(Bfr, 0, Cport.BytesToRead)
                    // sb.Append(Encoding.Default.GetString(Bfr))
                    sb = sb + Bfr;
                    System.Threading.Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                Interaction.MsgBox("The serial port has closed. Click OK to retry....", MsgBoxStyle.Critical, "VT105");
                try
                {
                    Cport.Close();
                }

                catch (Exception exa)
                {

                }
                Cport.Open();
                return "";
            }
            Dsync += 1;
            if (Dsync > 10)
            {
                VT105.UpdateDisplay();
                Dsync = 0;
            }
            return sb;
        }

        public bool IsConnected
        {
            get
            {
                return Cport.IsOpen;
            }
        }

    }
}