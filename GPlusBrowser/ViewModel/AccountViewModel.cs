using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Threading;

namespace GPlusBrowser.ViewModel
{
    using Model;

    public class AccountViewModel : ViewModelBase, IDisposable
    {
        public AccountViewModel(Account model, AccountManager manager, Dispatcher uiThreadDispatcher)
            : base(uiThreadDispatcher)
        {
            _accountModel = model;
            _accountModel.Initialized += _accountModel_Initialized;
            _accountManagerModel = manager;
            CircleSelecter = new CircleSeleterManagerViewModel(model.Stream, uiThreadDispatcher);
            Stream = new StreamManagerViewModel(model.Stream, uiThreadDispatcher);
            BackToAccountManagerCommand = new RelayCommand(BackToAccountManagerCommand_Execute);
        }

        Account _accountModel;
        AccountManager _accountManagerModel;
        CircleSeleterManagerViewModel _circleSelecter;
        StreamManagerViewModel _stream;
        Uri _accountIconUrl;

        public CircleSeleterManagerViewModel CircleSelecter
        {
            get { return _circleSelecter; }
            set
            {
                _circleSelecter = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("CircleSelecter"));
            }
        }
        public StreamManagerViewModel Stream
        {
            get { return _stream; }
            set
            {
                _stream = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Stream"));
            }
        }
        public Uri AccountIconUrl
        {
            get { return _accountIconUrl; }
            set
            {
                _accountIconUrl = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("AccountIconUrl"));
            }
        }
        public ICommand BackToAccountManagerCommand { get; private set; }
        public void Dispose()
        {
            Stream.Dispose();
        }

        void _accountModel_Initialized(object sender, EventArgs e)
        {
            AccountIconUrl = new Uri(_accountModel.MyProfile.IconImageUrlText.Replace("$SIZE_SEGMENT", "s35-c-k"));
        }
        void BackToAccountManagerCommand_Execute(object arg)
        {
            _accountManagerModel.SelectedAccountIndex = -1;
        }
    }
}