using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace PortScanner
{
    class Program
    {
        enum Protocol { Tcp, Udp, Both }

        class IPParams {
            public string server;
            public int port;
            public Protocol protocol;

            public IPParams(string server, int port, Protocol protocol) {
                this.server = server;
                this.port = port;
                this.protocol = protocol;
            }
        }

        public static void ConnectSocket(Object param)
        {
            IPParams p = (IPParams)param;
            IPHostEntry hostEntry = Dns.GetHostEntry(p.server);           
            foreach (IPAddress address in hostEntry.AddressList)
            {
                try
                {
                    IPEndPoint ipe = new IPEndPoint(address, p.port);
                    Socket tempSocket;
                    if(p.protocol == Protocol.Tcp)
                       tempSocket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    else
                       tempSocket = new Socket(ipe.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                    tempSocket.Connect(ipe);
                    if (tempSocket.Connected)
                    {
                        tempSocket.Close();
                        Console.WriteLine(ipe.Address.ToString() + ":" + ipe.Port.ToString() + " "
                            + p.protocol.ToString() + " - открыт");
                        break;
                    }
                    else
                    {
                        Console.WriteLine(ipe.Address.ToString() + ":" + ipe.Port.ToString() + " "
                            + p.protocol.ToString() + " - закрыт");
                        continue;
                    }
                }
                catch (SocketException e) {
                    if(e.SocketErrorCode == SocketError.TimedOut)
                        Console.WriteLine(address + ":" + p.port.ToString() +" " + p.protocol.ToString() + " - превышено время ожидания" );
                    else
                    if (e.SocketErrorCode == SocketError.AccessDenied)
                        Console.WriteLine(address + ":" + p.port.ToString() + " " + p.protocol.ToString() + " - доступ запрещен");
                    else
                    if (e.SocketErrorCode == SocketError.ConnectionReset || 
                        e.SocketErrorCode == SocketError.ConnectionAborted ||
                        e.SocketErrorCode == SocketError.ConnectionRefused)
                        Console.WriteLine(address + ":" + p.port.ToString() + " " + p.protocol.ToString() + " - сервер скидывает соединение");
                    else
                        Console.WriteLine(address + ":" + p.port.ToString() + " " + p.protocol.ToString() + " - ошибка при соединении");
                }                
            }                     
        }

        static void Main(string[] args)
        {
            string server = "yandex.ru";
            const int MAX_PORTS = 65535;
            int port_start = 1, port_stop = MAX_PORTS;
            Protocol protocol = Protocol.Both;
            if (args.Length != 4)
            {
                Console.Write("Введите адрес - ");
                server = Console.ReadLine();
                Console.Write("Введите начальный порт - ");
                port_start = int.Parse(Console.ReadLine());
                Console.Write("Введите конечный порт - ");
                port_stop = int.Parse(Console.ReadLine());
                Console.Write("Введите протокол -  0 Tcp 1 Udp 2 Both - ");
                protocol = (Protocol)int.Parse(Console.ReadLine());
            }
            else {
                server = args[0];
                port_start = int.Parse(args[1]);
                port_stop = int.Parse(args[2]);
                protocol = (Protocol)int.Parse(args[3]);
            }
            for (int port = port_start; port <= port_stop; ++port)
            {
                if (protocol == Protocol.Both || protocol == Protocol.Tcp)
                    new Thread(Program.ConnectSocket).Start(new IPParams(server, port, Protocol.Tcp));
                if (protocol == Protocol.Both || protocol == Protocol.Udp)
                    new Thread(Program.ConnectSocket).Start(new IPParams(server, port, Protocol.Udp));
            }
            Console.ReadKey(false);
        }
    }
}
