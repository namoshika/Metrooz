using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunokoLibrary.GooglePlus;

namespace GPlusBrowser.Model
{
    public class CircleManager
    {
        public CircleManager(Account mainWindow)
        {
            _mainWindow = mainWindow;
            _items = new List<CircleInfo>();
        }
        Account _mainWindow;
        List<CircleInfo> _items;

        public bool IsInitialized { get; private set; }
        public ReadOnlyCollection<CircleInfo> Items
        { get { return _items.AsReadOnly(); } }

        public async void Initialize()
        {
            try
            {
                await _mainWindow.GooglePlusClient.Relation
                    .UpdateCirclesAndBlockAsync(false).ConfigureAwait(false);
                lock (_items)
                {
                    _items.Clear();
                    _items.AddRange(_mainWindow.GooglePlusClient.Relation.Circles);
                }
                IsInitialized = true;
            }
            catch (FailToOperationException)
            { IsInitialized = false; }

            OnInitialized(new EventArgs());
        }
        public void ClipCircle(CircleInfo info) { }
        public Task<CircleInfo> CreateNew(string name) { return null; }
        public Task<bool> Remove(CircleInfo info) { return null; }
        public Task<bool> Move(int oldIndex, int newIndex) { return null; }

        public event EventHandler Initialized;
        protected virtual void OnInitialized(EventArgs e)
        {
            if (Initialized != null)
                Initialized(this, e);
        }
        public event NotifyCollectionChangedEventHandler ChangedItemsEvent;
        protected virtual void OnChangedItemsEvent(NotifyCollectionChangedEventArgs e)
        {
            if (ChangedItemsEvent != null)
                ChangedItemsEvent(this, e);
        }
    }
}