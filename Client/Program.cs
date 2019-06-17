using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Windows.Forms;
using Chat.Net;
using Client.Views;

namespace Client
{
    class Program
    {
        /// Точка входа клиента.
        /// Вызывает первую WinForm, которая затем вызывает Client и так далее
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.Run(new Welcome());
        }
    }
}
