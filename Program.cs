using System;
using System.IO.Ports;
using System.Text;

namespace SerialPortExample
{
    class SerialPortProgram
    {                
        static SerialPort _port;
        static void Main(string[] args)
        {
            string portName;
            portName = SetPortName();
            _port = new SerialPort(portName,57600, Parity.None, 8, StopBits.One);
            // Instatiate this class
            SerialPortProgram serialPortProgram = new SerialPortProgram();
        }

        private SerialPortProgram()
        {
            _port.DataReceived += new
              SerialDataReceivedEventHandler(port_DataReceived);
            OpenMyPort();
            Console.WriteLine("Type q to exit.");
            bool continueLoop = true;
            string inputCmd;

            while (continueLoop)
            {
                inputCmd = Console.ReadLine();

                if (inputCmd == "q")
                {
                    continueLoop = false;
                    break;
                }
                else
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(inputCmd);
                    var inputByte = new byte[] { 13 };
                    bytes = bytes.Concat(inputByte).ToArray();
                    _port.Write(bytes, 0, bytes.Length);
                }
            }
        }

        private void port_DataReceived(object sender,
          SerialDataReceivedEventArgs e)
        {           
            // Show all the incoming data in the port's buffer
            Console.WriteLine(_port.ReadExisting());
        }

        //Open selected COM port
        private static void OpenMyPort()
        {
            try
            {
                _port.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error opening my port: {0}", ex.Message);
                Environment.Exit(0);
            }
        }

        // Display Port values and prompt user to enter a port.
        public static string SetPortName()
        {
            string portName;

            Console.WriteLine("Available Ports:");
            foreach (string s in SerialPort.GetPortNames())
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Enter COM port value (ex: COM18): ");
            portName = Console.ReadLine();
            return portName;
        }

    }
}