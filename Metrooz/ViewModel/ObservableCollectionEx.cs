using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace SunokoLibrary.Collections.ObjectModel
{
    public static class ObservableCollectionEx
    {
        public static async Task AddOnDispatcher<T>(this ObservableCollection<T> collection, T item, Dispatcher dispatcher)
        { await dispatcher.InvokeAsync(() => collection.Add(item)); }
        public static async Task RemoveAtOnDispatcher<T>(this ObservableCollection<T> collection, int index, Dispatcher dispatcher)
        { await dispatcher.InvokeAsync(() => collection.RemoveAt(index)); }
        public static async Task InsertOnDispatcher<T>(this ObservableCollection<T> collection, int index, T item, Dispatcher dispatcher)
        { await dispatcher.InvokeAsync(() => collection.Insert(index, item)); }
        public static async Task ClearOnDispatcher<T>(this ObservableCollection<T> collection, Dispatcher dispatcher)
        { await dispatcher.InvokeAsync(collection.Clear); }
        public static async Task MoveOnDispatcher<T>(this ObservableCollection<T> collection, int indexA, int indexB, Dispatcher dispatcher)
        { await dispatcher.InvokeAsync(() => collection.Move(indexA, indexB)); }
        public static async Task RemoveOnDispatcher<T>(this ObservableCollection<T> collection, T item, Dispatcher dispatcher)
        { await dispatcher.InvokeAsync(() => collection.Remove(item)); }
        public static IDisposable SyncWith<TSource, TTarget>(this ObservableCollection<TSource> source, ObservableCollection<TTarget> target, Func<TSource, TTarget> converter, Action<Func<Task>, NotifyCollectionChangedEventArgs> outerProcInterceptor, Action<TTarget> removeProcInterceptor, Dispatcher dispatcher, int offset = 0)
        {
            
            var obs = Observable.Concat(
                Task.Run(async () =>
                    {
                        await target.ClearOnDispatcher(dispatcher);
                        for (var i = 0; i < offset; i++)
                            target.Add(default(TTarget));
                        for (var i = offset; i < source.Count + offset; i++)
                        {
                            var item = converter(source[i]);
                            await target.InsertOnDispatcher(i, item, dispatcher);
                        }
                    }).ToObservable(),
                Observable.FromEvent<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                    handler => (sender, e) => handler(e),
                    handler => source.CollectionChanged += handler,
                    handler => source.CollectionChanged -= handler)
                    .Do(e => outerProcInterceptor(async () =>
                    {
                        switch (e.Action)
                        {
                            case NotifyCollectionChangedAction.Add:
                                for (var i = 0; i < e.NewItems.Count; i++)
                                {
                                    var obj = (TSource)e.NewItems[i];
                                    await target.InsertOnDispatcher(e.NewStartingIndex + i + offset, converter(obj), dispatcher).ConfigureAwait(false);
                                }
                                break;
                            case NotifyCollectionChangedAction.Remove:
                                for (var i = 0; i < e.OldItems.Count; i++)
                                {
                                    var obj = target[e.OldStartingIndex + i + offset];
                                    await target.RemoveAtOnDispatcher(e.OldStartingIndex + i + offset, dispatcher).ConfigureAwait(false);
                                    removeProcInterceptor(obj);
                                }
                                break;
                            case NotifyCollectionChangedAction.Move:
                                await target.MoveOnDispatcher(e.OldStartingIndex + offset, e.NewStartingIndex + offset, dispatcher).ConfigureAwait(false);
                                break;
                            case NotifyCollectionChangedAction.Reset:
                                var objs = target.ToArray();
                                await target.ClearOnDispatcher(dispatcher).ConfigureAwait(false);
                                foreach (var item in objs)
                                    removeProcInterceptor(item);
                                break;
                        }
                    }, e)).Select(e => Unit.Default));
            return obs.Subscribe();
        }
    }
}