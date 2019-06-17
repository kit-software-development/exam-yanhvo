using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chat.Net;
using Chat.Auth;
using Chat.Exceptions;
using Chat.Chat;
using System.Threading;
using System.Net.Sockets;

namespace Server
{
    /// Класс сервера
    class Server : TCPServer
    {
        UserManager userManager;
        SessionManager sessionManager;
        ChatroomManager chatroomManager;
        
        public volatile Object readLock;

        /// Загрузка пользователей и чатов, сохраненных в файле
        public Server()
        {
            userManager = new UserManager();
            userManager.load("users.db");
            sessionManager = new SessionManager();
            chatroomManager = new ChatroomManager();
            chatroomManager.load("chatrooms.db");
            readLock = new Object();
        }

        public UserManager UserManager
        {
            get
            {
                return userManager;
            }

            set
            {
                userManager = value;
            }
        }

        public SessionManager SessionManager
        {
            get
            {
                return sessionManager;
            }

            set
            {
                sessionManager = value;
            }
        }

        public ChatroomManager ChatroomManager
        {
            get
            {
                return chatroomManager;
            }

            set
            {
                chatroomManager = value;
            }
        }

        /// Запуск сервера запускает 3 потока:
        /// Проверка сообщений, отправленных пользователями
        /// Проверка, вышел ли клиент
        /// Создание нового объекта TcpClient, если присоединяется новый клиент
        public void run()
        {
            checkDataThread = new Thread(new ThreadStart(this.checkData));
            checkDataThread.Start();

            checkQuitThread = new Thread(new ThreadStart(this.checkQuit));
            checkQuitThread.Start();

            listenerThread = new Thread(new ThreadStart(this.listen));
            listenerThread.Start();
        }

        /// Создание нового сеанса для каждого нового клиента с собственным экземпляром TcpClient
        private void listen()
        {
            while (this.Running)
            {
                try
                {
                    Console.WriteLine("Waiting for a new connection...");
                    TcpClient client = this.tcpListener.AcceptTcpClient();
                    Session session = new Session();
                    session.Client = client;
                    SessionManager.addSession(session);

                    Console.WriteLine("New client: " + session.Token);
                }
                catch (SocketException)
                {
                    // Ловим исключение WSACancelBlockingCall, потому что this.tcpListener, вероятно, закрыт
                    Console.WriteLine("Listener thread closed");
                }
            }
        }

        /// Проверка данных, поступающих от клиентов
        private void checkData()
        {
            while (this.Running)
            {
                try
                {
                    lock (readLock)
                    {
                        if (SessionManager.SessionList.Count > 0)
                        {
                            foreach (Session session in SessionManager.SessionList.ToList())
                            {
                                if (session != null && session.Client.GetStream().DataAvailable)
                                {
                                    Thread.Sleep(25);
                                    Message message = getMessage(session.Client.Client);

                                    if (message != null)
                                    {
                                        // Данные для обработки: вызов соответствующей функции
                                        Thread processData = new Thread(() => this.processData(session, message));
                                        processData.Start();
                                    }
                                }
                            }
                        }
                    }
                }
                catch(InvalidOperationException e)
                {
                    Console.WriteLine(e.Message);
                }
                
                Thread.Sleep(5);
            }
        }

        /// Проверка покинул ли клиент чат
        private void quitCr(Session session, Message message)
        {
            try
            {
                if (session.User.Chatroom != null)
                {
                    // Предупреждение пользователя, что он покинул чат
                    Message messageSuccess = new Message(Message.Header.QUIT_CR);
                    messageSuccess.addData("success");
                    messageSuccess.addData(session.User.Chatroom.Name);
                    sendMessage(messageSuccess, session.Client.Client);

                    // Предупреждение других пользователей, что он покинул чат
                    broadcastToChatRoom(session, "left the chatroom \"" + session.User.Chatroom.Name + "\"");

                    Console.WriteLine("- " + session.User.Login + " left the chatroom: " + session.User.Chatroom.Name);

                    session.User.Chatroom = null;
                }
            }
            catch (ChatroomUnknownException e)
            {
                // Предупреждение пользователя, что чат не существует
                Message messageError = new Message(Message.Header.QUIT_CR);
                messageError.addData("error");
                messageError.addData(message.MessageList[0]);
                sendMessage(messageError, session.Client.Client);

                messageError.addData("Chatroom " + e.Message + " does not exist");
            }
        }

        /// Обработка данных, отправленных клиентами
        private void processData(Session session, Message message)
        {
            if (session.User != null)
            {
                switch (message.Head)
                {
                    case Message.Header.QUIT:
                        {
                            // Предупреждение пользователя, что он был отключен
                            Message messageSuccess = new Message(Message.Header.QUIT);
                            messageSuccess.addData("success");
                            sendMessage(messageSuccess, session.Client.Client);

                            if(session.User.Chatroom != null)
                            {
                                // Предупреждение других пользователей, что он был отключен
                                broadcastToChatRoom(session, "left the chatroom \"" + session.User.Chatroom.Name + "\"");
                            }
                            
                            session.Client.Close();
                            sessionManager.removeSession(session.Token);

                            Console.WriteLine("- User logout: " + session.Token);
                        }
                        break;

                    case Message.Header.JOIN_CR:
                        // Прежде чем присоединиться к чату, покидает текущий
                        quitCr(session, message);

                        try
                        {
                            List<string> messageList = message.MessageList;
                            if (chatroomManager.ChatroomList.Any(x => x.Name == messageList[0]))
                            {
                                session.User.Chatroom = new Chatroom(messageList[0]);
                                Console.WriteLine("- " + session.User.Login + " joined the chatroom: " + messageList[0]);

                                // Сообщение клиенту, что канал был присоединен
                                Message messageSuccess = new Message(Message.Header.JOIN_CR);
                                messageSuccess.addData("success");
                                messageSuccess.addData(messageList[0]);
                                sendMessage(messageSuccess, session.Client.Client);

                                //Всем участникам чата сообщается о прибытии нового пользователя
                                Message messagePostBroadcast = new Message(Message.Header.POST);
                                broadcastToChatRoom(session, "joined the chatroom \"" + messageList[0] + "\"");
                            }
                        }
                        catch (ChatroomUnknownException e)
                        {
                            // Сообщение клиенту, что канал не был присоединен
                            Message messageSuccess = new Message(Message.Header.JOIN_CR);
                            messageSuccess.addData("error");
                            messageSuccess.addData(message.MessageList[0]);
                            sendMessage(messageSuccess, session.Client.Client);
                            messageSuccess.addData("Chatroom " + e.Message + " does not exist");
                        }
                        break;

                    case Message.Header.QUIT_CR:
                        quitCr(session, message);
                        break;

                    case Message.Header.CREATE_CR:
                        try
                        {
                            List<string> messageList = message.MessageList;
                            ChatroomManager.addChatroom(new Chatroom(messageList[0]));
                            ChatroomManager.save("chatrooms.db");

                            // Оповещение пользователя, что чат создан
                            Message messageSuccess = new Message(Message.Header.CREATE_CR);
                            messageSuccess.addData("success");
                            messageSuccess.addData(messageList[0]);
                            sendMessage(messageSuccess, session.Client.Client);

                            Console.WriteLine("- " + session.User.Login + " : chatroom has been created: " + messageList[0]);
                        }
                        catch (Exceptions e)
                        {
                            // Предупреждение пользователя, что чат не создан
                            Message messageError = new Message(Message.Header.CREATE_CR);
                            messageError.addData("error");
                            messageError.addData("Chatroom " + e.Message + " already exists");
                            sendMessage(messageError, session.Client.Client);
                        }
                        break;

                    case Message.Header.LIST_CR:
                        Message messageListCr = new Message(Message.Header.LIST_CR);

                        foreach (Chatroom chatroom in ChatroomManager.ChatroomList.ToList())
                        {
                            messageListCr.addData(chatroom.Name);
                        }

                        sendMessage(messageListCr, session.Client.Client);
                        break;

                    case Message.Header.POST:
                        Console.WriteLine("- " + session.User.Login + " : message received : " + message.MessageList[0]);
                        broadcastToChatRoom(session, message.MessageList[0]);
                        break;

                    case Message.Header.LIST_USERS:
                        List<string> chatroomWantedList = message.MessageList;
                        string chatroomWanted = chatroomWantedList[0];

                        Message messageListUsers = new Message(Message.Header.LIST_USERS);
                        
                        // Для всех подключенныз пользователей
                        foreach (Session localSession in SessionManager.SessionList.ToList())
                        {
                            // Если пользователь находится в чате, выводится список пользователей
                            if (localSession.User != null &&
                                localSession.User.Chatroom != null && 
                                localSession.User.Chatroom.Name == chatroomWanted)
                            {
                                messageListUsers.addData(localSession.User.Login);
                            }
                        }

                        sendMessage(messageListUsers, session.Client.Client);

                        break;
                }
            }
            else
            {
                switch (message.Head)
                {
                    case Message.Header.REGISTER:
                        try
                        {
                            List<string> messageList = message.MessageList;
                            UserManager.addUser(messageList[0], messageList[1]);
                            UserManager.save("users.db");

                            //  Предупреждение пользователя, что его учетная запись создана
                            Message messageSuccess = new Message(Message.Header.REGISTER);
                            messageSuccess.addData("success");
                            sendMessage(messageSuccess, session.Client.Client);

                            Console.WriteLine("- Registration success : " + messageList[0]);
                        }
                        catch (UserAlreadyExistsException e)
                        {
                            // Предупреждение пользователя, что его учетная запись не создана
                            Message messageSuccess = new Message(Message.Header.REGISTER);
                            messageSuccess.addData("error");
                            sendMessage(messageSuccess, session.Client.Client);

                            Console.WriteLine("- Registration failed : " + e.Message);
                        }
                        break;

                    case Message.Header.JOIN:
                        try
                        {
                            List<string> messageList = message.MessageList;
                            UserManager.authentify(messageList[0], messageList[1]);
                            session.User = new User(messageList[0], messageList[1]);
                            UserManager.save("users.db");

                            Message messageSuccess = new Message(Message.Header.JOIN);
                            messageSuccess.addData("success");
                            sendMessage(messageSuccess, session.Client.Client);

                            Console.WriteLine("- Login success : " + session.User.Login);
                        }
                        catch (WrongPasswordException e)
                        {
                            Message messageSuccess = new Message(Message.Header.JOIN);
                            messageSuccess.addData("error");
                            sendMessage(messageSuccess, session.Client.Client);
                            
                            Console.WriteLine("- Login failed : " + e.Message);
                        }

                        break;
                }
            }
        }

        /// Проверка ушел ли пользователь
        private void checkQuit()
        {
            while (this.Running)
            {
                for (int i = 0; i < SessionManager.SessionList.Count; i++)
                {
                    Socket socket = SessionManager.SessionList[i].Client.Client;

                    if (socket.Poll(10, SelectMode.SelectRead) && socket.Available == 0)
                    {
                        Console.WriteLine("- User logged out : " + SessionManager.SessionList[i].Token);

                        lock(readLock)
                        {
                            if (SessionManager.SessionList[i].User != null && 
                                SessionManager.SessionList[i].User.Chatroom != null)
                            {
                                // Оповезение остальных пользователей, что пользователб ушел
                                broadcastToChatRoom(SessionManager.SessionList[i], "left the chatroom \"" +
                                    SessionManager.SessionList[i].User.Chatroom.Name + "\"");
                            }

                            SessionManager.SessionList[i].Client.Close();
                            sessionManager.removeSession(SessionManager.SessionList[i].Token);
                        }
                    }
                }

                Thread.Sleep(5);
            }
        }

        /// Функция отправки сообщений всем пользователям чата
        private void broadcastToChatRoom(Session session, string message)
        {
            Chatroom chatroom = session.User.Chatroom;

            if(chatroom != null && message != "")
            {
                Message messageJoin = new Message(Message.Header.POST);
                messageJoin.addData(session.User.Login);
                messageJoin.addData(session.User.Login+": " + message);

                foreach(Session sessionUser in SessionManager.SessionList.ToList())
                {
                    if(sessionUser.User.Chatroom != null && 
                        sessionUser.User.Chatroom.Name == chatroom.Name)
                    {
                        sendMessage(messageJoin, sessionUser.Client.Client);
                    }
                }

                Console.WriteLine("- " + session.User.Login + "'s message broadcast");
            }
            else
            {
                Console.WriteLine("- User is not connected to any chatroom: " + session.User.Login);
            }
        }
    }
}
