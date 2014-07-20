using Livet;
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
        public static IDisposable SyncWith<TSource, TTarget>(this ObservableCollection<TSource> source, DispatcherCollection<TTarget> target, Func<TSource, TTarget> converter)
        {

            var obs = Observable.Concat(
                Task.Run(() =>
                    {
                        target.Clear();
                        for (var i = 0; i < source.Count; i++)
                        {
                            var item = converter(source[i]);
                            target.Insert(i, item);
                        }
                    }).ToObservable(),
                Observable.FromEvent<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                    handler => (sender, e) => handler(e),
                    handler => source.CollectionChanged += handler,
                    handler => source.CollectionChanged -= handler)
                    .Do(e =>
                        {
                            switch (e.Action)
                            {
                                case NotifyCollectionChangedAction.Add:
                                    for (var i = 0; i < e.NewItems.Count; i++)
                                    {
                                        var obj = (TSource)e.NewItems[i];
                                        target.Insert(e.NewStartingIndex + i, converter(obj));
                                    }
                                    break;
                                case NotifyCollectionChangedAction.Remove:
                                    for (var i = 0; i < e.OldItems.Count; i++)
                                    {
                                        var obj = target[e.OldStartingIndex + i];
                                        target.RemoveAt(e.OldStartingIndex + i);
                                        if (obj is IDisposable)
                                            ((IDisposable)obj).Dispose();
                                    }
                                    break;
                                case NotifyCollectionChangedAction.Move:
                                    target.Move(e.OldStartingIndex, e.NewStartingIndex);
                                    break;
                                case NotifyCollectionChangedAction.Reset:
                                    var objs = target.ToArray();
                                    target.Clear();
                                    foreach (var item in objs.OfType<IDisposable>())
                                        item.Dispose();
                                    break;
                            }
                        }).Select(e => Unit.Default));
            return obs.Subscribe();
        }
    }
}