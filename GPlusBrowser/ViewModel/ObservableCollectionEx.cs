using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace GPlusBrowser
{
    public static class ObservableCollectionEx
    {
        public static async Task AddOnDispatcher<T>(this ObservableCollection<T> collection, T item)
        { await App.Current.Dispatcher.InvokeAsync(() => collection.Add(item)); }
        public static async Task RemoveAtOnDispatcher<T>(this ObservableCollection<T> collection, int index)
        { await App.Current.Dispatcher.InvokeAsync(() => collection.RemoveAt(index)); }
        public static async Task InsertOnDispatcher<T>(this ObservableCollection<T> collection, int index, T item)
        { await App.Current.Dispatcher.InvokeAsync(() => collection.Insert(index, item)); }
        public static async Task ClearOnDispatcher<T>(this ObservableCollection<T> collection)
        { await App.Current.Dispatcher.InvokeAsync(collection.Clear); }
        public static async Task MoveOnDispatcher<T>(this ObservableCollection<T> collection, int indexA, int indexB)
        { await App.Current.Dispatcher.InvokeAsync(() => collection.Move(indexA, indexB)); }
        public static async Task RemoveOnDispatcher<T>(this ObservableCollection<T> collection, T item)
        { await App.Current.Dispatcher.InvokeAsync(() => collection.Remove(item)); }
    }
}