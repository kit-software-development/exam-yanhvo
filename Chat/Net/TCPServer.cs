﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chat.Net
{
    /// TCPServer
    [Serializable]
    public abstract class TCPServer
    {
        protected volatile TcpClient commSocket;
        protected volatile TcpListener tcpListener;
        protected volatile Boolean running;
        protected int port;
        protected Thread checkDataThread;
        protected Thread checkQuitThread;
        protected Thread listenerThread;

        public bool Running
        {
            get
            {
                return running;
            }

            set
            {
                running = value;
            }
        }

        /// Запуск сервера
        public void startServer(int port)
        {
            this.port = port;
            this.Running = false;
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");

            try
            {
                tcpListener = new TcpListener(ipAddress, port);
                tcpListener.Start();
                this.Running = true;
            }
            catch(SocketException e)
            {
                Console.WriteLine("Connection impossible: " + e.Message);
            }
            
        }

        /// Остановка сервера
        public void stopServer()
        {
            Console.WriteLine("Stopping the server");
            this.Running = false;
            tcpListener.Stop();
        }

        /// Получение объекта сообщения от данного клиента
        public Message getMessage(Socket socket)
        {
            Console.WriteLine("## TCPServer Receiving a message");

            try
            {
                NetworkStream strm = new NetworkStream(socket);
                IFormatter formatter = new BinaryFormatter();
                Message message = (Message)formatter.Deserialize(strm);
                Console.WriteLine("- message header: " + message.Head);
                return message;
            }
            catch (SerializationException e)
            {
                Console.WriteLine("TCPClient sendMessage exception: " + e.Message);
            }
            catch (IOException e)
            {
                Console.WriteLine("TCPClient sendMessage exception: " + e.Message);
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("TCPClient sendMessage exception: " + e.Message);
            }

            return null;
        }

        /// Отправка сообщения данному клиенту
        public void sendMessage(Message message, Socket socket)
        {
            Console.WriteLine("## TCPServer Sending a message: " + message.Head);

            try
            {
                IFormatter formatter = new BinaryFormatter();
                NetworkStream strm = new NetworkStream(socket);
                formatter.Serialize(strm, message);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("TCPClient sendMessage exception: " + e.Message);
            }
            catch (IOException e)
            {
                Console.WriteLine("TCPClient sendMessage exception: " + e.Message);
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("TCPClient sendMessage exception: " + e.Message);
            }
        }
    }
}
