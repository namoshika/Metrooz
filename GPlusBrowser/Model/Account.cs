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
            IsInitialized = false;
            Stream = new StreamManager(this);
            //Notification = new NotificationManager(this);
        }
        public bool IsInitialized { get; private set; }
        public IPlatformClientBuilder Builder { get; private set; }
        public PlatformClient PlusClient { get; private set; }
        public ProfileInfo MyProfile { get; private set; }
        public StreamManager Stream { get; private set; }
        //public NotificationManager Notification { get; private set; }
        readonly System.Threading.SemaphoreSlim _initSyncer = new System.Threading.SemaphoreSlim(1, 1);

        public async Task Initialize(bool isForced)
        {
            try
            {
                await _initSyncer.WaitAsync().ConfigureAwait(false);
                OnInitializing(new EventArgs());
                IsInitialized = false;

                if (IsInitialized && isForced == false)
                    return;
                if (PlusClient != null)
                    PlusClient.Dispose();

                //G+APIライブラリの初期化を行う
                PlusClient = await Builder.Build().ConfigureAwait(false);
                MyProfile = await PlusClient.People.GetProfileOfMeAsync(false).ConfigureAwait(false);

                //各モジュールの初期化を行う
                //Notification.Initialize();
                await Stream.Initialize().ConfigureAwait(false);
                Connect();

                IsInitialized = true;
            }
            finally
            {
                OnInitialized(new EventArgs());
                _initSyncer.Release();
            }
        }
        public void Connect()
        {
            //Stream.Connect();
            //Notification.Connect();
        }
        public void Dispose()
        {
            if (IsInitialized)
            {
                IsInitialized = false;
                PlusClient.Dispose();
                Stream.Dispose();
            }
        }

        public event EventHandler Initializing;
        protected virtual void OnInitializing(EventArgs e)
        {
            if (Initializing != null)
                Initializing(this, e);
        }
        public event EventHandler Initialized;
        protected virtual void OnInitialized(EventArgs e)
        {
            if (Initialized != null)
                Initialized(this, e);
        }
    }
}