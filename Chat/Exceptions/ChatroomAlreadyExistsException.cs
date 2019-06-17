using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat.Exceptions
{
    public class ChatroomAlreadyExistsException : ChatroomException
    {
        public ChatroomAlreadyExistsException(string message) : base(message)
        {
        }

        public ChatroomAlreadyExistsException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }

    public class WrongPasswordException : AuthentificationException
    {
        public WrongPasswordException(string message) : base(message)
        {
        }

        public WrongPasswordException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }

    public class ChatroomException : System.Exception
    {
        public ChatroomException(string message) : base(message)
        {
        }

        public ChatroomException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }

    public class ChatroomUnknownException : ChatroomException
    {
        public ChatroomUnknownException(string message) : base(message)
        {
        }

        public ChatroomUnknownException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }
    public class AuthentificationException : System.Exception
    {
        public AuthentificationException(string message) : base(message)
        {
        }

        public AuthentificationException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }

    public class RegisterException : System.Exception
    {
        public RegisterException(string message) : base(message)
        {
        }

        public RegisterException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }

    public class SessionAlreadyExistsException : AuthentificationException
    {
        public SessionAlreadyExistsException(string message) : base(message)
        {
        }

        public SessionAlreadyExistsException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }

    public class SessionUnknownException : AuthentificationException
    {
        public SessionUnknownException(string message) : base(message)
        {
        }

        public SessionUnknownException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }

    public class UserAlreadyExistsException : AuthentificationException
    {
        public UserAlreadyExistsException(string message) : base(message)
        {
        }

        public UserAlreadyExistsException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }

    public class UserUnknownException : AuthentificationException
    {
        public UserUnknownException(string message) : base(message)
        {
        }

        public UserUnknownException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }
}
