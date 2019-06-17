using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Chat.Exceptions;

namespace Chat.Auth
{
    /// Обработка пользователей
    public class UserManager
    {
        List<User> userList;

        /// Хранение пользователей в списке
        public List<User> UserList
        {
            get
            {
                return userList;
            }

            set
            {
                userList = value;
            }
        }

        public UserManager()
        {
            UserList = new List<User>();
        }

        /// Добавление пользователя в список. Проверка, что он уже не существует с данным логином.
        public void addUser(string login, string password)
        {
            foreach (User user in UserList.ToList())
            {
                if (user.Login == login)
                {
                    throw new UserAlreadyExistsException(login);
                }
            }

            UserList.Add(new User(login, password));
        }

        /// Удаление пользователя из списка на основе его логина
        public void removeUser(string login)
        {
            User userToDelete = null;

            foreach(User user in UserList.ToList())
            {
                if(user.Login == login)
                {
                    userToDelete = user;
                }
            }

            if(userToDelete == null)
            {
                throw new UserUnknownException(login);
            }

            UserList.Remove(userToDelete);
        }

        /// Поиск пользователя в списке
        public User getUser(User other)
        {
            User getUser = null;

            foreach (User user in UserList.ToList())
            {
                if (user.Login == other.Login && user.Password == other.Password)
                {
                    getUser = user;
                }
            }

            if (getUser == null)
            {
                throw new UserUnknownException(other.Login);
            }

            return getUser;
        }

        /// Аутентификация пользователя на основе его пароля.
        public void authentify(string login, string password)
        {
            User userToAuthentify = null;

            foreach (User user in UserList.ToList())
            {
                if(user.Login == login && user.Password == password)
                {
                    userToAuthentify = user;
                }
            }

            if(userToAuthentify == null)
            {
                throw new WrongPasswordException(login);
            }
        }

        /// Загрузка пользователей из файла
        public void load(string path)
        {
            try
            {
                // десериализация из файла
                using (Stream stream = File.Open(path, FileMode.Open))
                {
                    BinaryFormatter bin = new BinaryFormatter();
                    List<User> users = (List<User>)bin.Deserialize(stream);
                    userList = users;
                }
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// Сохранение текущего списока в файл.
        public void save(string path)
        {
            try
            {
                // получаем поток, куда будем записывать сериализованный объект
                using (Stream stream = File.Open(path, FileMode.Create))
                {
                    BinaryFormatter bin = new BinaryFormatter();
                    bin.Serialize(stream, UserList);
                }
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
