using SunokoLibrary.Web.GooglePlus;
using SunokoLibrary.Web.GooglePlus.Primitive;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPlusBrowser.Model
{
    public class AccountManager : IDisposable
    {
        public AccountManager()
        {
            _accounts = new ObservableCollection<Account>();
            _readonlyAccounts = new ReadOnlyObservableCollection<Account>(_accounts);
        }
        ObservableCollection<Account> _accounts;
        ReadOnlyObservableCollection<Account> _readonlyAccounts;
        public ReadOnlyCollection<Account> Accounts { get { return _readonlyAccounts; } }

        public async Task Initialize()
        {
            foreach (var item in _accounts)
                item.Dispose();
            _accounts.Clear();
            foreach (var item in await PlatformClient.Factory.ImportFromChrome())
            {
                var account = new Account(item);
                _accounts.Add(account);
            }
        }
        public void Dispose()
        {
            foreach (var item in Accounts)
                item.Dispose();
        }
    }
}