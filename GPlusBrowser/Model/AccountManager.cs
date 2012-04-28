using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace GPlusBrowser.Model
{
    public class AccountManager : IDisposable
    {
        public AccountManager()
        {
            _accounts = new List<Account>();
            SettingManager = new SettingModelManager();
            SelectedAccountIndex = -1;
        }
        List<Account> _accounts;
        int _selectedAccountIndex;
        public int SelectedAccountIndex
        {
            get { return _selectedAccountIndex; }
            set
            {
                _selectedAccountIndex = value;
                OnChangedSelectedAccountIndex(new EventArgs());
            }
        }
        public SettingModelManager SettingManager { get; private set; }
        public ReadOnlyCollection<Account> Accounts { get { return _accounts.AsReadOnly(); } }

        public void Add(Account item)
        {
            _accounts.Add(item);
            SettingManager.Items.Add(item.Setting);
            SettingManager.Save();
            OnChangedAccounts(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Add, item, _accounts.Count - 1));
        }
        public void Remove(Account item)
        {
            var idx = _accounts.IndexOf(item);
            _accounts.RemoveAt(idx);
            OnChangedAccounts(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Remove, item, idx));
        }
        public Account Create()
        {
            var setting = new SettingModel();
            var account = new Account(setting);
            return account;
        }
        public async void Initialize()
        {
            foreach (var item in _accounts)
                item.Dispose();
            _accounts.Clear();
            await SettingManager.Reload();
            foreach (var item in SettingManager.Items)
            {
                var account = new Model.Account(item);
                _accounts.Add(account);
                OnChangedAccounts(new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add, account, _accounts.Count - 1));
            }
        }
        public void Dispose()
        {
            SettingManager.Save().Wait();
        }

        public event EventHandler ChangedSelectedAccountIndex;
        protected virtual void OnChangedSelectedAccountIndex(EventArgs e)
        {
            if (ChangedSelectedAccountIndex != null)
                ChangedSelectedAccountIndex(this, e);
        }
        public event NotifyCollectionChangedEventHandler ChangedAccounts;
        protected virtual void OnChangedAccounts(NotifyCollectionChangedEventArgs e)
        {
            if (ChangedAccounts != null)
                ChangedAccounts(this, e);
        }
    }
}
