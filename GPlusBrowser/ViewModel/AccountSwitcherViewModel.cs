using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace GPlusBrowser.ViewModel
{
    using Model;

    public class AccountSwitcherViewModel : ViewModelBase, IDisposable
    {
        public AccountSwitcherViewModel(AccountManager accountManagerModel, Dispatcher uiThreadDispatcher)
            : base(uiThreadDispatcher)
        {
            _accountManagerModel = accountManagerModel;
            _accountManagerModel.ChangedAccounts += _accountManagerModel_ChangedAccounts;
            _accountManagerModel.ChangedSelectedAccountIndex += _accountManagerModel_ChangedSelectedAccountIndex;
            _selectedSubPageIndex = -1;
            MainPages = new ObservableCollection<object>();
            MainPages.Add(new AccountManagerViewModel(accountManagerModel, uiThreadDispatcher));
            SubPages = new ObservableCollection<object>();
        }
        AccountManager _accountManagerModel;
        bool _isShowSidePanel;
        int _selectedMainPageIndex, _selectedSubPageIndex;

        public bool IsShowSidePanel
        {
            get { return _isShowSidePanel; }
            set
            {
                _isShowSidePanel = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("IsShowSidePanel"));
            }
        }
        public int SelectedMainPageIndex
        {
            get { return _selectedMainPageIndex; }
            set
            {
                if (_selectedMainPageIndex == value)
                    return;
                _selectedMainPageIndex = value;
                _accountManagerModel.SelectedAccountIndex = _selectedMainPageIndex - 1;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("SelectedMainPageIndex"));
            }
        }
        public int SelectedSubPageIndex
        {
            get { return _selectedSubPageIndex; }
            set
            {
                if (_selectedSubPageIndex == value)
                    return;
                _selectedSubPageIndex = value;
                _accountManagerModel.SelectedAccountIndex = _selectedMainPageIndex - 1;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("SelectedSubPageIndex"));
            }
        }
        public ObservableCollection<object> MainPages { get; set; }
        public ObservableCollection<object> SubPages { get; set; }

        void _accountManagerModel_ChangedAccounts(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (var i = 0; i < e.NewItems.Count; i++)
                    {
                        MainPages.InsertAsync(e.NewStartingIndex + i + 1, new AccountViewModel((Account)e.NewItems[i], _accountManagerModel, UiThreadDispatcher), UiThreadDispatcher);
                        SubPages.InsertAsync(e.NewStartingIndex + i, new NotificationManagerViewModel(((Account)e.NewItems[i]).Notification, UiThreadDispatcher), UiThreadDispatcher);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    for (var i = 0; i < e.OldItems.Count; i++)
                    {
                        MainPages.RemoveAtAsync(e.OldStartingIndex + 1, UiThreadDispatcher);
                        SubPages.RemoveAtAsync(e.OldStartingIndex, UiThreadDispatcher);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    var accountManager = MainPages.First();
                    MainPages.Clear();
                    MainPages.AddAsync(accountManager, UiThreadDispatcher);
                    SelectedSubPageIndex = -1;
                    break;
            }
        }
        void _accountManagerModel_ChangedSelectedAccountIndex(object sender, EventArgs e)
        {
            UiThreadDispatcher.BeginInvoke((Action)delegate()
            {
                if (SelectedMainPageIndex == _accountManagerModel.SelectedAccountIndex + 1)
                    return;
                SelectedMainPageIndex = _accountManagerModel.SelectedAccountIndex + 1;
                SelectedSubPageIndex = _accountManagerModel.SelectedAccountIndex;
            },
            _accountManagerModel.SelectedAccountIndex < MainPages.Count - 1
                ? DispatcherPriority.DataBind : DispatcherPriority.ContextIdle);
        }

        public void Dispose()
        {
            foreach (AccountViewModel item in MainPages.Skip(1))
                item.Dispose();
        }
    }
}