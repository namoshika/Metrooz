﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace GPlusBrowser.ViewModel
{
    using Model;

    public class AccountPanelViewModel : ViewModelBase
    {
        public AccountPanelViewModel(Account accountModel, AccountManager accountManagerModel, AccountViewModel topLevel, Dispatcher uiThreadDispatcher)
            : base(uiThreadDispatcher, topLevel)
        {
            _accountModel = accountModel;
            _accountModel.Initialized += _accountModel_Initialized;
            _accountManagerModel = accountManagerModel;
            _setting = new SettingViewModel(accountModel.Setting, accountModel, topLevel, uiThreadDispatcher);
            _userName = _accountModel.Setting.UserName;
            AsyncFunc();
            OpenStreamPanelCommand = new RelayCommand(OpenStreamPanelCommand_Execute);
        }
        Account _accountModel;
        AccountManager _accountManagerModel;
        SettingViewModel _setting;
        System.Windows.Media.ImageSource _userIconUrl;
        string _userName;

        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("UserName"));
            }
        }
        public ImageSource UserIconUrl
        {
            get { return _userIconUrl; }
            set
            {
                _userIconUrl = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("UserIconUrl"));
            }
        }
        public SettingViewModel Setting
        {
            get { return _setting; }
            set
            {
                _setting = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Setting"));
            }
        }
        public ICommand OpenStreamPanelCommand { get; private set; }

        async void AsyncFunc()
        {
            if (_accountModel.AccountIconUrl != null)
                UserIconUrl = await DataCacheDictionary.Default.DownloadImage(
                    new Uri(_accountModel.AccountIconUrl.Replace("$SIZE_SEGMENT", "s35-c-k")));
        }
        async void _accountModel_Initialized(object sender, EventArgs e)
        {
            UserName = _accountModel.Setting.UserName;
            UserIconUrl = await DataCacheDictionary.Default.DownloadImage(
                new Uri(_accountModel.AccountIconUrl.Replace("$SIZE_SEGMENT", "s35-c-k")));
        }
        async void OpenStreamPanelCommand_Execute(object arg)
        {
            var targetIdx = _accountManagerModel.Accounts.IndexOf(_accountModel);
            _accountManagerModel.SelectedAccountIndex = targetIdx;

            var seqStatus = _accountManagerModel.Accounts[targetIdx].InitializeSequenceStatus;
            if (seqStatus != AccountInitSeqStatus.UnLogined && seqStatus  < AccountInitSeqStatus.LoadedHomeInit)
                await _accountManagerModel.Accounts[targetIdx].Initialize().ConfigureAwait(false);
        }
    }
}