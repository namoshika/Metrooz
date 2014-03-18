using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunokoLibrary.Threading;
using SunokoLibrary.Web.GooglePlus;
using SunokoLibrary.Web.GooglePlus.Primitive;

namespace GPlusBrowser.Model
{
    public class Account : IDisposable
    {
        public Account(IPlatformClientBuilder setting)
        {
            Builder = setting;
            Stream = new StreamManager(this);
            Notification = new NotificationManager(this);
        }
        readonly System.Threading.SemaphoreSlim _initSyncer = new System.Threading.SemaphoreSlim(1, 1);

        public IPlatformClientBuilder Builder { get; private set; }
        public PlatformClient PlusClient { get; private set; }
        public ProfileInfo MyProfile { get; private set; }
        public StreamManager Stream { get; private set; }
        public NotificationManager Notification { get; private set; }

        public async Task Activate()
        {
            await Deactivate();
            try
            {
                await _initSyncer.WaitAsync().ConfigureAwait(false);

                //G+APIライブラリの初期化を行う
                PlusClient = await Builder.Build().ConfigureAwait(false);
                MyProfile = await PlusClient.People.GetProfileOfMeAsync(false).ConfigureAwait(false);

                //各モジュールの初期化を行う
                await Notification.Activate().ConfigureAwait(false);
                await Stream.Activate().ConfigureAwait(false);
            }
            finally { _initSyncer.Release(); }
        }
        public async Task Deactivate()
        {
            try
            {
                await _initSyncer.WaitAsync().ConfigureAwait(false);
                await Notification.Deactivate().ConfigureAwait(false);
                await Stream.Deactivate().ConfigureAwait(false);
                if (PlusClient != null)
                    PlusClient.Dispose();
                MyProfile = null;
            }
            finally { _initSyncer.Release(); }
        }
        public void Dispose()
        {
            Stream.Dispose();
            Notification.Dispose();
            if (PlusClient != null)
                PlusClient.Dispose();
        }
    }
}