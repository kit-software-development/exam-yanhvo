using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Chat.Net
{
    /// Хранение сообщения
    /// Каждое сообщение состоит из заголовка, который описывает его (регистрация пользователя, присоединение к чату, выход из чата, публикация сообщения и т. д)
    /// Затем его содержимое сохраняется в списке строк
    [Serializable]
    public class Message
    {
        public enum Header { REGISTER, JOIN, QUIT, JOIN_CR, QUIT_CR, CREATE_CR, LIST_CR, POST, LIST_USERS }
        private Header head;
        private List<string> messageList;

        public Header Head
        {
            get
            {
                return head;
            }

            set
            {
                head = value;
            }
        }

        public List<string> MessageList
        {
            get
            {
                return messageList;
            }

            set
            {
                messageList = value;
            }
        }

        public Message(Header head, string message)
        {
            this.Head = head;
            this.MessageList = new List<string>();
            this.MessageList.Add(message);
        }

        public Message(Header head)
        {
            this.Head = head;
            this.MessageList = new List<string>();
        }

        public Message(Header head, List<string> messages)
        {
            this.Head = head;
            this.MessageList = new List<string>();
            this.MessageList = messages;
        }

        /// Добавление данных в список сообщений
        public void addData(string message)
        {
            this.MessageList.Add(message);
        }
    }
}
