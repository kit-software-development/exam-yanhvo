﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    /// Список двусторонней привязки данных для работы с потоками
    public class ThreadedBindingList<T> : BindingList<T>
    {
        private readonly SynchronizationContext ctx;
        public ThreadedBindingList()
        {
            ctx = SynchronizationContext.Current;
        }
        protected override void OnAddingNew(AddingNewEventArgs e)
        {
            SynchronizationContext ctx = SynchronizationContext.Current;
            if (ctx == null)
            {
                BaseAddingNew(e);
            }
            else
            {
                try
                {
                    ctx.Send(delegate
                    {
                        BaseAddingNew(e);
                    }, null);
                }
                catch (Exception)
                {
                    Console.WriteLine("Error");
                }
            }
        }
        void BaseAddingNew(AddingNewEventArgs e)
        {
            base.OnAddingNew(e);
        }
        protected override void OnListChanged(ListChangedEventArgs e)
        {
            if (ctx == null)
            {
                BaseListChanged(e);
            }
            else
            {
                try
                {
                    ctx.Send(delegate
                    {
                        BaseListChanged(e);
                    }, null);
                }
                catch (Exception)
                {
                    Console.WriteLine("Error");
                }

            }
        }
        void BaseListChanged(ListChangedEventArgs e)
        {
            base.OnListChanged(e);
        }
    }
}
