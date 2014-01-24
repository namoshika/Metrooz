using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace GPlusBrowser.ViewModel
{
    using Model;

    public class AccountViewModel : ViewModelBase
    {
        public AccountViewModel(Account model)
        {
            _accountModel = model;
            _accountModel.Initialized += _accountModel_Initialized;
            _userName = _accountModel.Builder.Name;
            DataCacheDictionary.DownloadImage(new Uri(_accountModel.Builder.IconUrl.Replace("$SIZE_SEGMENT", "s35-c-k")))
                .ContinueWith(tsk => UserIconUrl = tsk.Result);

            OpenStreamPanelCommand = new RelayCommand(OpenStreamPanelCommand_Execute);
            BackToAccountManagerCommand = new RelayCommand(BackToAccountManagerCommand_Execute);
            ConnectStreamCommand = new RelayCommand(ConnectStreamCommand_Execute);
        }
        Account _accountModel;
        StreamManagerViewModel _stream;
        BitmapImage _userIconUrl;
        string _userName;

        public string UserName
        {
            get { return _userName; }
            set { Set(() => UserName, ref _userName, value); }
        }
        public BitmapImage UserIconUrl
        {
            get { return _userIconUrl; }
            set { Set(() => UserIconUrl, ref _userIconUrl, value); }
        }
        public StreamManagerViewModel Stream
        {
            get { return _stream; }
            set { Set(() => Stream, ref _stream, value); }
        }
        public ICommand OpenStreamPanelCommand { get; private set; }
        public ICommand BackToAccountManagerCommand { get; private set; }
        public ICommand ConnectStreamCommand { get; private set; }
        public override void Cleanup()
        {
            base.Cleanup();
            _accountModel.Initialized -= _accountModel_Initialized;
            if (_stream != null)
                _stream.Cleanup();
        }

        async void _accountModel_Initialized(object sender, EventArgs e)
        {
            if (_accountModel.IsInitialized == false)
                Stream.SelectedCircleIndex = -1;
            else
            {
                Stream = new StreamManagerViewModel(_accountModel.Stream, _accountModel);
                //Notification = new NotificationManagerViewModel(_accountModel.Notification, this, UiThreadDispatcher);
                UserName = _accountModel.MyProfile.Name;
                UserIconUrl = await DataCacheDictionary.DownloadImage(
                    new Uri(_accountModel.Builder.IconUrl
                        .Replace("$SIZE_SEGMENT", "s35-c-k")
                        .Replace("$SIZE_NUM", "80")));
            }
        }
        async void OpenStreamPanelCommand_Execute()
        {
            OnOpenedStreamPanel(new EventArgs());
            await _accountModel.Initialize(false);
        }
        void BackToAccountManagerCommand_Execute()
        { OnBackedToAccountManager(new EventArgs()); }
        void ConnectStreamCommand_Execute()
        { _accountModel.Connect(); }

        public event EventHandler OpenedStreamPanel;
        protected virtual void OnOpenedStreamPanel(EventArgs e)
        {
            if (OpenedStreamPanel != null)
                OpenedStreamPanel(this, e);
        }
        public event EventHandler BackedToAccountManager;
        protected virtual void OnBackedToAccountManager(EventArgs e)
        {
            if (BackedToAccountManager != null)
                BackedToAccountManager(this, e);
        }
    }
}