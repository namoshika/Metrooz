using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunokoLibrary.Web.GooglePlus;

namespace GPlusBrowser.Model
{
    public class Account : IDisposable
    {
        public Account(SettingModel setting)
        {
            Setting = setting;            
            Notification = new NotificationManager(this);
            AccountIconUrl = setting.UserIconUrl;
            InitializeSequenceStatus = setting.Cookies == null
                ? AccountInitSeqStatus.UnLogined : AccountInitSeqStatus.Logined;
        }

        public TalkGadgetBindStatus IsConnected { get; private set; }
        public AccountInitSeqStatus InitializeSequenceStatus { get; private set; }
        public PlatformClient PlusClient { get; private set; }
        public CircleManager Circles { get; private set; }
        public StreamManager Stream { get; private set; }
        public SettingModel Setting { get; private set; }
        public NotificationManager Notification { get; private set; }
        public ProfileInfo MyProfile { get; private set; }
        public string AccountIconUrl { get; private set; }

        public async Task Initialize()
        {
            if (InitializeSequenceStatus < AccountInitSeqStatus.Logined)
                throw new InvalidOperationException("InitializeSequenceStatusがLoginedを満たさない状態でのInitialize()呼び出しは無効です。");

            try
            {
                PlusClient = await PlatformClient.Create(Setting.Cookies).ConfigureAwait(false);
                InitializeSequenceStatus = AccountInitSeqStatus.Logined;
                PlusClient.Activity.ChangedIsConnected += Activity_ChangedIsConnected;
                await PlusClient.Relation.UpdateCirclesAndBlockAsync(false, CircleUpdateLevel.Loaded).ConfigureAwait(false);
                MyProfile = await PlusClient.Relation.GetProfileOfMeAsync(false).ConfigureAwait(false);
                Circles = new CircleManager(this);
                Stream = new StreamManager(this);
                InitializeSequenceStatus = AccountInitSeqStatus.LoadedHomeInit;

                AccountIconUrl = MyProfile.IconImageUrlText;
                if (Setting.UserIconUrl != MyProfile.IconImageUrlText)
                    Setting.UserIconUrl = MyProfile.IconImageUrlText;
                if (Setting.UserName != MyProfile.Name)
                    Setting.UserName = MyProfile.Name;

                Notification.Initialize();
                await Circles.Update();
                InitializeSequenceStatus = AccountInitSeqStatus.LoadedFullDatas;
            }
            catch (FailToOperationException)
            { InitializeSequenceStatus = AccountInitSeqStatus.DisableSession; }

            OnInitialized(new EventArgs());
        }
        public async Task Login(string mail, string password)
        {
            try
            {
                var cookie = new System.Net.CookieContainer();
                var client = await PlatformClient.Create(cookie, mail, password).ConfigureAwait(false);
                var profileIcon = (await client.Relation.GetProfileOfMeAsync(false).ConfigureAwait(false)).IconImageUrlText;
                InitializeSequenceStatus = AccountInitSeqStatus.Logined;
                Setting.MailAddress = mail;
                Setting.UserIconUrl = profileIcon;
                Setting.Cookies = cookie;
                Setting.Save();
                AccountIconUrl = profileIcon;
            }
            catch (FailToOperationException)
            { InitializeSequenceStatus = AccountInitSeqStatus.UnLogined; }
        }
        public void Connect()
        {
            IsConnected = TalkGadgetBindStatus.Disconnected;
            Notification.Connect();
        }
        public void Dispose()
        {
            OnDisposed(new EventArgs());
        }
        void Activity_ChangedIsConnected(object sender, EventArgs e)
        {
            IsConnected = PlusClient.Activity.IsConnected
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
    { UnLogined, Logined, LoadedHomeInit, LoadedFullDatas, DisableSession, }
    public enum TalkGadgetBindStatus
    { Disconnected, Connected, DisableConnect, }
}