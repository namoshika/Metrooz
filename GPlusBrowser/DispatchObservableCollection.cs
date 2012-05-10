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
    public class DispatchObservableCollection<T> : ObservableCollection<T>
    {
        public DispatchObservableCollection(Dispatcher dispatcher) { EventDispatcher = dispatcher; }
        public DispatchObservableCollection(IEnumerable<T> collection, Dispatcher dispatcher)
            : base(collection) { EventDispatcher = dispatcher; }
        public DispatchObservableCollection(List<T> list, Dispatcher dispatcher)
            : base(list) { EventDispatcher = dispatcher; }

        // CollectionChangedイベントを発行するときに使用するディスパッチャ
        public Dispatcher EventDispatcher { get; set; }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (IsValidAccess())
            {
                // UIスレッドならそのまま実行
                base.OnCollectionChanged(e);
            }
            else
            {
                // UIスレッドじゃなかったらDispatcherにお願いする
                Action<NotifyCollectionChangedEventArgs> changed = base.OnCollectionChanged;
                EventDispatcher.Invoke(changed, e);
            }
        }

        // UIスレッドからのアクセスかどうかを判定する
        private bool IsValidAccess()
        {
            return EventDispatcher.Thread == Thread.CurrentThread;
        }
    }
}