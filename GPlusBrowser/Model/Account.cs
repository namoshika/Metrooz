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
            IsConnected = false;
            IsInitialized = false;
            Stream = new StreamManager(this);
        }
        public bool IsConnected { get; private set; }
        public bool IsInitialized { get; private set; }
        public IPlatformClientBuilder Builder { get; private set; }
        public PlatformClient PlusClient { get; private set; }
        public ProfileInfo MyProfile { get; private set; }
        public StreamManager Stream { get; private set; }
        //public NotificationManager Notification { get; private set; }
        readonly AsyncLocker _initSyncer = new AsyncLocker();

        public async Task Initialize(bool isForced)
        {
            await _initSyncer.LockAsync(isForced, () => isForced || IsInitialized == false, null,
                async () =>
                {
                    if (PlusClient != null)
                    {
                        PlusClient.Activity.ChangedIsConnected -= ActivityManager_ChangedIsConnected;
                        PlusClient.Dispose();
                    }
                    try
                    {
                        //G+APIライブラリの初期化を行う
                        PlusClient = await Builder.Build();
                        MyProfile = await PlusClient.Relation.GetProfileOfMeAsync(false).ConfigureAwait(false);

                        //各モジュールの初期化を行う
                        Stream = new StreamManager(this);
                        //Notification = new NotificationManager(this);
                        //Notification.Initialize();
                        await Stream.Initialize(CircleUpdateLevel.Loaded);
                        Connect();
                        PlusClient.Activity.ChangedIsConnected += ActivityManager_ChangedIsConnected;

                        IsInitialized = true;
                        OnInitialized(new EventArgs());
                    }
                    catch (FailToOperationException)
                    { IsInitialized = false; }
                }, null);
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
                PlusClient.Activity.ChangedIsConnected -= ActivityManager_ChangedIsConnected;
                PlusClient.Dispose();
                Stream.Dispose();
            }
        }
        void ActivityManager_ChangedIsConnected(object sender, EventArgs e)
        {
            IsConnected = PlusClient.Activity.IsConnected;
            OnChangedConnectStatus(new EventArgs());
        }

        public event EventHandler Initialized;
        protected virtual void OnInitialized(EventArgs e)
        {
            if (Initialized != null)
                Initialized(this, e);
        }
        public event EventHandler ChangedConnectStatus;
        protected virtual void OnChangedConnectStatus(EventArgs e)
        {
            if (ChangedConnectStatus != null)
                ChangedConnectStatus(this, e);
        }
    }
}