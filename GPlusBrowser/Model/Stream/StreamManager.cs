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
    public class StreamManager : IDisposable
    {
        public StreamManager(Account account)
        {
            _accountModel = account;
            Streams = new ObservableCollection<Stream>();
        }
        Account _accountModel;
        readonly SemaphoreSlim _streamSyncer = new SemaphoreSlim(1, 1);

        public ObservableCollection<Stream> Streams { get; private set; }
        public async Task Activate()
        {
            await _accountModel.PlusClient.People.UpdateCirclesAndBlockAsync(false, CircleUpdateLevel.Loaded);
            try
            {
                await _streamSyncer.WaitAsync();
                Streams.Add(new Stream(this) { Circle = _accountModel.PlusClient.People.YourCircle });

                var i = 0;
                for (; i < _accountModel.PlusClient.People.Circles.Count; i++)
                {
                    var circleInf = _accountModel.PlusClient.People.Circles[i];
                    var item = Streams.FirstOrDefault(info => info.Circle.Id == circleInf.Id);
                    if (item != null)
                        Streams.Move(Streams.IndexOf(item), i + 1);
                    else
                        Streams.Insert(i + 1, new Stream(this) { Circle = circleInf });
                }
                for (i += 1; i < Streams.Count; i++)
                {
                    var rmItem = Streams[i];
                    Streams.RemoveAt(i);
                    rmItem.Dispose();
                }
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
        public async void Dispose()
        {
            try
            {
                await _streamSyncer.WaitAsync().ConfigureAwait(false);
                foreach (var item in Streams)
                    item.Dispose();
            }
            finally { _streamSyncer.Release(); }
        }
    }
}