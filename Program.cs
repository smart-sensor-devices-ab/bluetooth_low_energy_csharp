using System;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace SerialPortExample
{
    internal class Program
    {
        private static SerialPort? _port;
        private static volatile bool _running = true;

        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            string portName = ChoosePort();
            if (string.IsNullOrWhiteSpace(portName))
            {
                Console.WriteLine("No port selected. Exiting.");
                return;
            }

            _port = new SerialPort(portName, 57600, Parity.None, 8, StopBits.One)
            {
                Encoding = Encoding.ASCII,   
                NewLine = "\r\n",            
                ReadTimeout = 200,
                WriteTimeout = 1000,

                DtrEnable = true,
                RtsEnable = true,
                Handshake = Handshake.None
            };

            try
            {
                _port.Open();
                Thread.Sleep(200);

                Console.WriteLine($"Opened {portName} @ 57600 8N1 (CRLF).");
                Console.WriteLine("Type AT commands and press Enter. Type q to quit.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to open {portName}: {ex.Message}");
                return;
            }

            var reader = new Thread(ReadLoop) { IsBackground = true };
            reader.Start();

            SendLine("AT");

            while (true)
            {
                var line = Console.ReadLine();
                if (line == null) continue;

                if (string.Equals(line.Trim(), "q", StringComparison.OrdinalIgnoreCase))
                    break;

                SendLine(line);
            }

            _running = false;
            try { _port.Close(); } catch { }
        }

        private static void SendLine(string text)
        {
            if (_port == null || !_port.IsOpen) return;

            try
            {
                Console.WriteLine($">> {text}");

                _port.Write(text + _port.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Write error: {ex.Message}");
            }
        }

        private static void ReadLoop()
        {
            if (_port == null) return;

            var sb = new StringBuilder();
            while (_running && _port.IsOpen)
            {
                try
                {
                    string incoming = _port.ReadExisting();
                    if (!string.IsNullOrEmpty(incoming))
                    {
                        Console.Write(incoming);
                    }
                    else
                    {
                        Thread.Sleep(20);
                    }
                }
                catch (TimeoutException)
                {
                    // ignore
                }
                catch
                {
                    Thread.Sleep(50);
                }
            }
        }

        private static string ChoosePort()
        {
            string[] ports;
            try
            {
                ports = SerialPort.GetPortNames();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not list COM ports: " + ex.Message);
                return "";
            }

            Console.WriteLine("Available Ports:");
            if (ports.Length == 0)
            {
                Console.WriteLine("  (none found)");
                Console.Write("Type the port manually (ex: COM18): ");
                return Console.ReadLine() ?? "";
            }

            Array.Sort(ports, StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < ports.Length; i++)
                Console.WriteLine($"  [{i + 1}] {ports[i]}");

            Console.Write("Select number (or type COM name like COM18): ");
            var input = (Console.ReadLine() ?? "").Trim();

            if (input.StartsWith("COM", StringComparison.OrdinalIgnoreCase))
                return input;

            if (int.TryParse(input, out int idx) && idx >= 1 && idx <= ports.Length)
                return ports[idx - 1];

            Console.Write("Invalid selection. Type port manually (ex: COM18): ");
            return Console.ReadLine() ?? "";
        }
    }
}