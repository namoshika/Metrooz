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
        { App.Current.Dispatcher.Invoke((Action<T>)collection.Add, item); }
        public static void RemoveAtOnDispatcher<T>(this ObservableCollection<T> collection, int index)
        { App.Current.Dispatcher.Invoke((Action<int>)collection.RemoveAt, index); }
        public static void InsertOnDispatcher<T>(this ObservableCollection<T> collection, int index, T item)
        { App.Current.Dispatcher.Invoke((Action<int, T>)collection.Insert, index, item); }
        public static void ClearOnDispatcher<T>(this ObservableCollection<T> collection)
        { App.Current.Dispatcher.Invoke((Action)collection.Clear); }
        public static void MoveOnDispatcher<T>(this ObservableCollection<T> collection, int indexA, int indexB)
        { App.Current.Dispatcher.Invoke((Action<int, int>)collection.Move, indexA, indexB); }
        public static void RemoveOnDispatcher<T>(this ObservableCollection<T> collection, T item)
        { App.Current.Dispatcher.Invoke((Func<T, bool>)collection.Remove, item); }
    }
}