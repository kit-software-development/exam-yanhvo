﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Chat.Auth
{

    /// Создание сеанса для каждого пользователя

    public class Session
    {
        Guid token;
        User user;
        TcpClient client;

        /// Каждый сеанс имеет уникальный идентификатор 
        public Guid Token
        {
            get
            {
                return token;
            }

            set
            {
                token = value;
            }
        }

        public User User
        {
            get
            {
                return user;
            }

            set
            {
                user = value;
            }
        }

        public TcpClient Client
        {
            get
            {
                return client;
            }

            set
            {
                client = value;
            }
        }

        public Session()
        {
            this.Token = Guid.NewGuid();
            this.Client = null;
            this.User = null;
        }
    }
}
