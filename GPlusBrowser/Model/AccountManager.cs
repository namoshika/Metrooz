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
            SettingManager = new SettingModelManager();
        }
        ObservableCollection<Account> _accounts;
        ReadOnlyObservableCollection<Account> _readonlyAccounts;
        public SettingModelManager SettingManager { get; private set; }
        public ReadOnlyCollection<Account> Accounts { get { return _readonlyAccounts; } }

        public async Task Initialize()
        {
            try
            {
                foreach (var item in _accounts)
                    item.Dispose();
                _accounts.Clear();
                SettingManager.Reload();
                foreach (var item in await PlatformClient.Factory.ImportFromChrome())
                {
                    var account = new Account(item);
                    _accounts.Add(account);
                }
            }
            catch (Exception e)
            { throw new ApplicationException("AccountManagerの初期化に失敗しました。", e); }
        }
        public void Dispose()
        { SettingManager.Save(); }
    }
}