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
            AccountIconUrl = setting.UserIconUrl;
            IsInitialized = false;
        }
        ~Account() { Dispose(); }

        //Application OwnerApplicationModel{get;private set;}
        //AccountManager AccountManagerModel{get;private set;}
        public bool IsLogined { get; private set; }
        public bool IsInitialized { get; private set; }
        public PlatformClient GooglePlusClient { get; private set; }
        public CircleManager Circles { get; private set; }
        public StreamManager Stream { get; private set; }
        public SettingModel Setting { get; private set; }
        //public NotificationManager Notification{get;private set;}
        //public ShareBoxManager ShareBox{get;private set;}
        public AccountProfileInfo MyProfile { get; private set; }
        public string AccountIconUrl { get; private set; }

        public async Task Initialize()
        {
            var client = new PlatformClient(Setting.Cookies);
            try
            {
                await client.UpdateHomeInitDataAsync().ConfigureAwait(false);
                IsLogined = true;

                GooglePlusClient = client;
                Circles.Initialize();
                MyProfile = await client.Relation.GetProfileOfMe(false).ConfigureAwait(false);
                AccountIconUrl = MyProfile.IconImageUrlText;
                if (Setting.UserIconUrl != MyProfile.IconImageUrlText)
                    Setting.UserIconUrl = MyProfile.IconImageUrlText;
                if (Setting.UserName != MyProfile.Name)
                    Setting.UserName = MyProfile.Name;
                IsInitialized = true;
            }
            catch (FailToUpdateException)
            { IsLogined = false; }

            OnInitialized(new EventArgs());
        }
        public async System.Threading.Tasks.Task<bool> Login(string mail, string password)
        {
            var cookie = new System.Net.CookieContainer();
            if (IsLogined = await PlatformClient.TryLogin(mail, password, cookie).ConfigureAwait(false))
            {
                Setting.MailAddress = mail;
                Setting.Cookies = cookie;
                GooglePlusClient = new PlatformClient(Setting.Cookies);
                MyProfile = await GooglePlusClient.Relation.GetProfileOfMe(false).ConfigureAwait(false);
                Setting.UserName = MyProfile.Name;
                Setting.UserIconUrl = MyProfile.IconImageUrlText;
                AccountIconUrl = MyProfile.IconImageUrlText;
            }
            OnChangedLoginStatus(new EventArgs());
            return IsLogined;
        }
        public void Dispose()
        {
            System.GC.SuppressFinalize(this);
            OnDisposed(new EventArgs());
        }

        void Circles_Initialized(object sender, EventArgs e)
        {
            Stream.Initialize();
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
        public event EventHandler ChangedLoginStatus;
        protected virtual void OnChangedLoginStatus(EventArgs e)
        {
            if (ChangedLoginStatus != null)
                ChangedLoginStatus(this, e);
        }
    }
}
