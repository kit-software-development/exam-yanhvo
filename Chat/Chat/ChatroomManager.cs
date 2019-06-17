using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chat.Exceptions;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Chat.Chat
{
    /// Обработка всех чатов
    [Serializable]
    public class ChatroomManager
    {
        List<Chatroom> chatroomList;

        /// Создание списка всех чатов
        public List<Chatroom> ChatroomList
        {
            get
            {
                return chatroomList;
            }

            set
            {
                chatroomList = value;
            }
        }

        public ChatroomManager()
        {
            ChatroomList = new List<Chatroom>();
        }

        /// Добавление чата, если его еще нет в спике
        public void addChatroom(Chatroom other)
        {
            foreach (Chatroom chatroom in ChatroomList.ToList())
            {
                if (chatroom.Name == other.Name)
                {
                    throw new Exceptions.Exceptions(chatroom.Name);
                }
            }

            ChatroomList.Add(other);
        }

        /// Удаление чата, основываясь на его имени
        public void removeChatroom(string name)
        {
            Chatroom chatroomToDelete = null;

            foreach (Chatroom chatroom in ChatroomList.ToList())
            {
                if (chatroom.Name == name)
                {
                   chatroomToDelete = chatroom;
                }
            }

            if (chatroomToDelete == null)
            {
                throw new ChatroomUnknownException(name);
            }

            ChatroomList.Remove(chatroomToDelete);
        }

        /// Загрузка чатов, хранящихся в файле
        public void load(string path)
        {
            try
            {
                using (Stream stream = File.Open(path, FileMode.Open))
                {
                    BinaryFormatter bin = new BinaryFormatter();
                    List<Chatroom> chatrooms = (List<Chatroom>)bin.Deserialize(stream);
                    chatroomList = chatrooms;
                }
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// Сохранение текущих чатов в файл
        public void save(string path)
        {
            try
            {
                using (Stream stream = File.Open(path, FileMode.Create))
                {
                    BinaryFormatter bin = new BinaryFormatter();
                    bin.Serialize(stream, ChatroomList);
                }
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
