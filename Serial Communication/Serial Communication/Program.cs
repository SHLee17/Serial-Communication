using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;

namespace SerialPortApp
{
    class Program
    {
        static SerialPort _serialPort;
        static bool _continue = true;
        static string logFile;

        static void Main(string[] args)
        {
            string log = Path.Combine(Directory.GetCurrentDirectory(), "Log");
            NewFolder(log, "LogFolder");
            logFile = Path.Combine(log, "Log");
            NewFile(logFile, "Log");


            string[] portNames = GetPortNames();
            StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
            string message;
            int portId;

            int baudRate;
            Parity parity;
            int dataBits;
            StopBits stopBits;

            _serialPort = new SerialPort();

            //1.통신정보설정  
            Console.WriteLine("포트를 선택해주세요.");
            for (int i = 0; i < portNames.Length; i++)
            {
                Console.Write("{0}:{1} ", i + 1, portNames[i]);
            }
            Console.WriteLine();

            message = Console.ReadLine();
            portId = Int32.Parse(message);

            // Set the read/write timeouts
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;
            PortSettings(portNames[portId - 1], 9600, Parity.None, 8, StopBits.One);

            //2.포트오픈
            _serialPort.Open();
            Console.WriteLine("PORT OPEN:{0}", _serialPort.PortName);

            //3.데이터수신
            //  수신대기스레드생성
            Thread readThread = new Thread(Read);
            readThread.Start();

            //4. 데이터송신
            Console.WriteLine("Type QUIT to exit");
            while (_continue)
            {
                message = Console.ReadLine();
                if (stringComparer.Equals("quit", message))
                {
                    _continue = false;
                }
                else
                {
                    Send(message);
                }
            }

            //수신쓰레드종료까지대기
            readThread.Join();
            _serialPort.Close();

        }

        //포트리스트가져오기
        static string[] GetPortNames()
        {
            return System.IO.Ports.SerialPort.GetPortNames();
        }

        //포트리스트가져오기
        static void PortSettings(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            _serialPort.PortName = portName;
            _serialPort.BaudRate = baudRate;
            _serialPort.DataBits = dataBits;
            _serialPort.Parity = parity;
            _serialPort.StopBits = stopBits;
        }

        //데이터송신
        static void Send(string sendMsg)
        {
            _serialPort.WriteLine(sendMsg);
        }

        //데이터수신스레드부
        static void Read()
        {
            string readData = string.Empty;
            Console.WriteLine("Read Ready");

            while (_continue)
            {
                try
                {
                    readData = _serialPort.ReadLine();
                    Console.WriteLine("RD:{0}", readData);

                    File.WriteAllText(logFile, readData);
                }
                catch (System.TimeoutException)
                {
                }
            }
        }
        static void NewFile(string path, string name)
        {
            if (File.Exists(path))
            {
                Console.WriteLine("중복된 파일 이름이 존재합니다.");
            }
            else
            {
                // 파일 생성
                File.WriteAllText(path, "");
                Console.WriteLine($"{name} 파일이 생성되었습니다.");
            }
        }
        static void NewFolder(string path, string name)
        {
            if (!Directory.Exists(path))
            {
                // 폴더가 없는 경우 폴더 만들기
                Directory.CreateDirectory(path);
                Console.WriteLine($"New {name} folder created.");
            }
            else
            {
                Console.WriteLine($"{name} Folder already exists.");
            }
            Console.WriteLine($"{name} Folder path: " + path);
        }
    }
}