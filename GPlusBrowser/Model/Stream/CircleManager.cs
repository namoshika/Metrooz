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
            Items = new ObservableCollection<CircleInfo>();
            UpdateStatus = CircleUpdateSeqState.Unloaded;
        }
        Account _accountModel;
        public CircleUpdateSeqState UpdateStatus { get; private set; }
        public ObservableCollection<CircleInfo> Items { get; private set; }
        public async Task Initialize(CircleUpdateLevel loadMode)
        {
            await _accountModel.PlusClient.Relation.UpdateCirclesAndBlockAsync(false, loadMode).ConfigureAwait(false);
            lock (Items)
            {
                var idx = 0;
                for (; idx < _accountModel.PlusClient.Relation.Circles.Count; idx++)
                {
                    //PlatformClientはサークル情報更新時にCircles.CircleInfoの同一性を保持しない
                    //そのため、ストリームの遅延読み込みでCircleInfoの新旧の扱いに面倒な部分がある。
                    //ここの処理でストリームの遅延読み込みに必要なCircleInfoの新旧の追跡を実現する
                    var item = _accountModel.PlusClient.Relation.Circles[idx];
                    var oldItem = Items.FirstOrDefault(info => info.Id == item.Id);
                    if (oldItem != null)
                    {
                        //旧CircleInfoがある場合は場所替えと新しいものへの差し替えを行う
                        Items.Move(Items.IndexOf(oldItem), idx);
                        Items[idx] = item;
                    }
                    else
                        Items.Insert(idx, item);
                }
                for (; idx < Items.Count; idx++)
                    Items.RemoveAt(idx);

                UpdateStatus = loadMode == CircleUpdateLevel.Loaded
                    ? CircleUpdateSeqState.Loaded : CircleUpdateSeqState.FullLoaded;
            }
            OnUpdated(new EventArgs());
        }

        public event EventHandler Updated;
        protected virtual void OnUpdated(EventArgs e)
        {
            if (Updated != null)
                Updated(this, e);
        }
    }
    public enum CircleUpdateSeqState
    { Unloaded, Loaded, FullLoaded }
}