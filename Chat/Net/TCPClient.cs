using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Chat.Net
{
    /// Каждый клиент имеет экземпляр TCPClient, который содержит объект TcpClient
    /// а также IPAddress, порт сервера и логическое значение, используемое для определения момента выхода
    [Serializable]
    public abstract class TCPClient
    {
        protected int port;
        private Boolean quit;
        protected TcpClient tcpClient;
        protected IPAddress ipAddress;

        public int Port
        {
            get
            {
                return port;
            }

            set
            {
                port = value;
            }
        }

        public IPAddress IpAddress
        {
            get
            {
                return ipAddress;
            }

            set
            {
                ipAddress = value;
            }
        }

        public bool Quit
        {
            get
            {
                return quit;
            }

            set
            {
                quit = value;
            }
        }

        public TCPClient()
        {
            tcpClient = null;
            IpAddress = null;
        }

        public void setServer(IPAddress ipAddress, int port)
        {
            this.Port = port;
            this.IpAddress = ipAddress;
        }

        /// Подключение к серверу
        public void connect()
        {
            try
            {
                tcpClient = new TcpClient(ipAddress.ToString(), Port);
            }
            catch(SocketException e)
            {
                Quit = true;
                Console.WriteLine("Connection refused by the server: " + e.Message);
                throw new Exception(e.Message);
            }
        }

        /// Получение сообщения с сервера.
        /// Десериализация полученного сообщения и возврат его
        public Message getMessage()
        {
            if (!Quit)
            {
                try
                {
                    NetworkStream strm = tcpClient.GetStream();
                    IFormatter formatter = new BinaryFormatter();
                    Message message = (Message)formatter.Deserialize(strm);
                    Console.WriteLine("## TCPClient Receiving a message: " + message.Head);
                    return message;
                }
                catch (SerializationException e)
                {
                    Console.WriteLine("TCPClient sendMessage exception: " + e.Message);
                    Quit = true;
                }
                catch (IOException e)
                {
                    Console.WriteLine("TCPClient sendMessage exception: " + e.Message);
                    Quit = true;
                }
                catch (InvalidOperationException e)
                {
                    Console.WriteLine("TCPClient sendMessage exception: " + e.Message);
                    Quit = true;
                }
            }

            return null;
        }

        /// Отправка сообщения не сервер
        public void sendMessage(Message message)
        {
            if (!Quit)
            {
                Console.WriteLine("## TCPClient Sending a message: " + message.Head);

                try
                {
                    IFormatter formatter = new BinaryFormatter();
                    NetworkStream strm = tcpClient.GetStream();
                    formatter.Serialize(strm, message);
                }
                catch (SerializationException e)
                {
                    Console.WriteLine("TCPClient sendMessage exception: " + e.Message);
                    Quit = true;
                }
                catch (IOException e)
                {
                    Console.WriteLine("TCPClient sendMessage exception: " + e.Message);
                    Quit = true;
                }
                catch (InvalidOperationException e)
                {
                    Console.WriteLine("TCPClient sendMessage exception: " + e.Message);
                    Quit = true;
                }
            }
        }
    }
}