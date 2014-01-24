using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;

namespace GPlusBrowser
{
    public static class ObservableCollectionEx
    {
        public static void AddOnDispatcher<T>(this ObservableCollection<T> collection, T item)
        { App.Current.Dispatcher.Invoke((Action<T>)collection.Add, DispatcherPriority.ContextIdle, item); }
        public static void RemoveAtOnDispatcher<T>(this ObservableCollection<T> collection, int index)
        { App.Current.Dispatcher.Invoke((Action<int>)collection.RemoveAt, DispatcherPriority.ContextIdle, index); }
        public static void InsertOnDispatcher<T>(this ObservableCollection<T> collection, int index, T item)
        { App.Current.Dispatcher.Invoke((Action<int, T>)collection.Insert, DispatcherPriority.ContextIdle, index, item); }
        public static void ClearOnDispatcher<T>(this ObservableCollection<T> collection)
        { App.Current.Dispatcher.Invoke((Action)collection.Clear, DispatcherPriority.ContextIdle); }
        public static void MoveOnDispatcher<T>(this ObservableCollection<T> collection, int indexA, int indexB)
        { App.Current.Dispatcher.Invoke((Action<int, int>)collection.Move, DispatcherPriority.ContextIdle, indexA, indexB); }
        public static void RemoveOnDispatcher<T>(this ObservableCollection<T> collection, T item)
        { App.Current.Dispatcher.Invoke((Func<T, bool>)collection.Remove, DispatcherPriority.ContextIdle, item); }
        public static System.Threading.Tasks.Task<T> GetFromIndexOnDispatcher<T>(this ObservableCollection<T> collection, int index)
        {
            return System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    var waiter = new System.Threading.AutoResetEvent(false);
                    T result = default(T);
                    var aaa = App.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (index >= 0 && index < collection.Count)
                            result = collection[index];
                        waiter.Set();
                    }), DispatcherPriority.ContextIdle);
                    waiter.WaitOne();
                    return result;
                });
        }
    }
}