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
        }
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
                        PlusClient.Dispose();
                    try
                    {
                        //G+APIライブラリの初期化を行う
                        PlusClient = await Builder.Build();
                        MyProfile = await PlusClient.People.GetProfileOfMeAsync(false).ConfigureAwait(false);

                        //各モジュールの初期化を行う
                        Stream = new StreamManager(this);
                        //Notification = new NotificationManager(this);
                        //Notification.Initialize();
                        await Stream.Initialize();
                        Connect();

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
                PlusClient.Dispose();
                Stream.Dispose();
            }
        }

        public event EventHandler Initialized;
        protected virtual void OnInitialized(EventArgs e)
        {
            if (Initialized != null)
                Initialized(this, e);
        }
    }
}