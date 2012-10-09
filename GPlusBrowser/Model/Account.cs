using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunokoLibrary.GooglePlus;

namespace GPlusBrowser.Model
{
    public class Account
    {
        public Account(SettingModel setting)
        {
            Setting = setting;
            GooglePlusClient = new PlatformClient(setting.Cookies);
            Circles = new CircleManager(this);
            Circles.Initialized += Circles_Initialized;
            Stream = new StreamManager(this);
            Notification = new NotificationManager(this);
            AccountIconUrl = setting.UserIconUrl;
            InitializeSequenceStatus = setting.Cookies == null
                ? AccountInitSeqStatus.UnLogined : AccountInitSeqStatus.Logined;
        }
        ~Account() { Dispose(); }

        //Application OwnerApplicationModel{get;private set;}
        //AccountManager AccountManagerModel{get;private set;}
        public TalkGadgetBindStatus IsConnected { get; private set; }
        public AccountInitSeqStatus InitializeSequenceStatus { get; private set; }
        public PlatformClient GooglePlusClient { get; private set; }
        public CircleManager Circles { get; private set; }
        public StreamManager Stream { get; private set; }
        public SettingModel Setting { get; private set; }
        public NotificationManager Notification { get; private set; }
        //public ShareBoxManager ShareBox{get;private set;}
        public ProfileInfo MyProfile { get; private set; }
        public string AccountIconUrl { get; private set; }

        public async Task Initialize()
        {
            if (InitializeSequenceStatus < AccountInitSeqStatus.Logined)
                throw new InvalidOperationException("InitializeSequenceStatusがLoginedを満たさない状態でのInitialize()呼び出しは無効です。");

            var client = new PlatformClient(Setting.Cookies);
            try
            {
                InitializeSequenceStatus = AccountInitSeqStatus.Logined;
                var initDtTask = client.UpdateHomeInitDataAsync().ContinueWith(tsk => true).ConfigureAwait(false);
                var profileTask = client.Relation.GetProfileOfMe(false).ConfigureAwait(false);

                GooglePlusClient = client;
                GooglePlusClient.Activity.ChangedIsConnected += Activity_ChangedIsConnected;
                MyProfile = await profileTask;
                InitializeSequenceStatus = AccountInitSeqStatus.LoadedProfile;
                if (await initDtTask)
                    InitializeSequenceStatus = AccountInitSeqStatus.LoadedHomeInit;
                Circles.Initialize();
                Notification.Initialize();
                AccountIconUrl = MyProfile.IconImageUrlText;
                if (Setting.UserIconUrl != MyProfile.IconImageUrlText)
                    Setting.UserIconUrl = MyProfile.IconImageUrlText;
                if (Setting.UserName != MyProfile.Name)
                    Setting.UserName = MyProfile.Name;
            }
            catch (FailToOperationException)
            {
                InitializeSequenceStatus = AccountInitSeqStatus.DisableSession;
            }

            OnInitialized(new EventArgs());
        }
        public async Task Login(string mail, string password)
        {
            var cookie = new System.Net.CookieContainer();
            InitializeSequenceStatus = await PlatformClient.TryLogin(mail, password, cookie)
                .ConfigureAwait(false) ? AccountInitSeqStatus.Logined : AccountInitSeqStatus.UnLogined;
            if (InitializeSequenceStatus == AccountInitSeqStatus.Logined)
                try
                {
                    Setting.MailAddress = mail;
                    Setting.Cookies = cookie;
                    GooglePlusClient = new PlatformClient(Setting.Cookies);
                    MyProfile = await GooglePlusClient.Relation.GetProfileOfMe(false).ConfigureAwait(false);
                    Setting.UserName = MyProfile.Name;
                    Setting.UserIconUrl = MyProfile.IconImageUrlText;
                    AccountIconUrl = Setting.UserIconUrl;
                    InitializeSequenceStatus = AccountInitSeqStatus.LoadedProfile;
                }
                catch (FailToOperationException)
                { }
        }
        public void Reconnect()
        {
            IsConnected = TalkGadgetBindStatus.Disconnected;
            Notification.Connect();
            foreach (var item in Stream.DisplayStreams)
                item.Connect();
        }
        public void Dispose()
        {
            System.GC.SuppressFinalize(this);
            OnDisposed(new EventArgs());
        }

        void Circles_Initialized(object sender, EventArgs e)
        {
            InitializeSequenceStatus = AccountInitSeqStatus.LoadedFullDatas;
            Stream.Initialize();
        }
        void Activity_ChangedIsConnected(object sender, EventArgs e)
        {
            IsConnected = GooglePlusClient.Activity.IsConnected
              ? TalkGadgetBindStatus.Connected : TalkGadgetBindStatus.DisableConnect;
            OnChangedConnectStatus(new EventArgs());
        }

        public event EventHandler Initialized;
        protected virtual void OnInitialized(EventArgs e)
        {
            if (Initialized != null)
                Initialized(this, e);
        }
        public event EventHandler Disposed;
        protected virtual void OnDisposed(EventArgs e)
        {
            if (Disposed != null)
                Disposed(this, e);
        }
        public event EventHandler ChangedConnectStatus;
        protected virtual void OnChangedConnectStatus(EventArgs e)
        {
            if (ChangedConnectStatus != null)
                ChangedConnectStatus(this, e);
        }
    }
    public enum AccountInitSeqStatus
    {
        UnLogined = 0, Logined = 1, LoadedProfile = 2, LoadedHomeInit = 3,
        LoadedFullDatas = 4, DisableSession = 5,
    }
    public enum TalkGadgetBindStatus
    { Disconnected, Connected, DisableConnect }
}