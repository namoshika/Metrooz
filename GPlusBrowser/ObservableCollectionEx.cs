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
        public static void Add<T>(this ObservableCollection<T> collection, T item, Dispatcher dispacher)
        { dispacher.BeginInvoke((Action<T>)collection.Add, DispatcherPriority.ContextIdle, item); }
        public static void RemoveAt<T>(this ObservableCollection<T> collection, int index, Dispatcher dispacher)
        { dispacher.BeginInvoke((Action<int>)collection.RemoveAt, DispatcherPriority.ContextIdle, index); }
        public static void Insert<T>(this ObservableCollection<T> collection, int index, T item, Dispatcher dispacher)
        { dispacher.BeginInvoke((Action<int, T>)collection.Insert, DispatcherPriority.ContextIdle, index, item); }
        public static void Clear<T>(this ObservableCollection<T> collection, Dispatcher dispacher)
        { dispacher.BeginInvoke((Action)collection.Clear, DispatcherPriority.ContextIdle); }
        public static void Move<T>(this ObservableCollection<T> collection, int indexA, int indexB, Dispatcher dispacher)
        { dispacher.BeginInvoke((Action<int, int>)collection.Move, DispatcherPriority.ContextIdle, indexA, indexB); }
        public static void Remove<T>(this ObservableCollection<T> collection, T item, Dispatcher dispacher)
        { dispacher.BeginInvoke((Func<T, bool>)collection.Remove, DispatcherPriority.ContextIdle, item); }
    }
}