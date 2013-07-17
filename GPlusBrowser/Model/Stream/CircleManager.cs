using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunokoLibrary.Web.GooglePlus;

namespace GPlusBrowser.Model
{
    public class CircleManager
    {
        public CircleManager(Account mainWindow)
        {
            _accountModel = mainWindow;
            _items = new ObservableCollection<CircleInfo>();
            Items = new ReadOnlyObservableCollection<CircleInfo>(_items);
            UpdateStatus = CircleUpdateSeqState.Unloaded;
        }
        Account _accountModel;
        ObservableCollection<CircleInfo> _items;
        public CircleUpdateSeqState UpdateStatus { get; private set; }
        public ReadOnlyObservableCollection<CircleInfo> Items { get; private set; }
        public async Task Update(CircleUpdateLevel loadMode = CircleUpdateLevel.Loaded)
        {
            try
            {
                await _accountModel.PlusClient.Relation
                    .UpdateCirclesAndBlockAsync(false, loadMode)
                    .ConfigureAwait(false);

                lock (_items)
                {
                    var idx = 0;
                    for (; idx < _accountModel.PlusClient.Relation.Circles.Count; idx++)
                    {
                        //PlatformClientはサークル情報更新時にCircles.CircleInfoの同一性を保持しない
                        //そのため、ストリームの遅延読み込みでCircleInfoの新旧の扱いに面倒な部分がある。
                        //ここの処理でストリームの遅延読み込みに必要なCircleInfoの新旧の追跡を実現する
                        var item = _accountModel.PlusClient.Relation.Circles[idx];
                        var oldItem = _items.FirstOrDefault(info => info.Id == item.Id);
                        if (oldItem != null)
                        {
                            //旧CircleInfoがある場合は場所替えと新しいものへの差し替えを行う
                            _items.Move(_items.IndexOf(oldItem), idx);
                            _items[idx] = item;
                        }
                        else
                            _items.Insert(idx, item);
                    }
                    for (; idx < _items.Count; idx++)
                        _items.RemoveAt(idx);

                    UpdateStatus = loadMode == CircleUpdateLevel.Loaded
                        ? CircleUpdateSeqState.Loaded : CircleUpdateSeqState.FullLoaded;
                }
                OnUpdated(new EventArgs());
            }
            catch (FailToUpdateException)
            { UpdateStatus = CircleUpdateSeqState.Failed; }
        }

        public event EventHandler Updated;
        protected virtual void OnUpdated(EventArgs e)
        {
            if (Updated != null)
                Updated(this, e);
        }
        public event NotifyCollectionChangedEventHandler ChangedItemsEvent;
        protected virtual void OnChangedItemsEvent(NotifyCollectionChangedEventArgs e)
        {
            if (ChangedItemsEvent != null)
                ChangedItemsEvent(this, e);
        }
    }
    public enum CircleUpdateSeqState
    { Unloaded, Loaded, FullLoaded, Failed }
}