using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;
using SunokoLibrary.Web.GooglePlus;

namespace GPlusBrowser.Model
{
    public class StreamManager
    {
        public StreamManager(Account account)
        {
            _accountModel = account;
            Streams = new ObservableCollection<Stream>();
        }
        readonly SemaphoreSlim _streamSyncer = new SemaphoreSlim(1, 1);
        Account _accountModel;

        public ObservableCollection<Stream> Streams { get; private set; }
        public async Task Activate()
        {
            await Deactivate();
            await _accountModel.PlusClient.People.UpdateCirclesAndBlockAsync(false, CircleUpdateLevel.Loaded);
            try
            {
                await _streamSyncer.WaitAsync();

                Streams.Add(new Stream(_accountModel.PlusClient.People.YourCircle, this));
                foreach(var item in _accountModel.PlusClient.People.Circles)
                    Streams.Add(new Stream(item, this));
            }
            finally { _streamSyncer.Release(); }
        }
        public async Task Deactivate()
        {
            try
            {
                await _streamSyncer.WaitAsync();
                var tmp = Streams.ToArray();
                Streams.Clear();
                foreach (var item in tmp)
                    item.Dispose();
            }
            finally { _streamSyncer.Release(); }
        }
    }
}