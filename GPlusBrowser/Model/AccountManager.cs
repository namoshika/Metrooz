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
            Accounts = new ObservableCollection<Account>();
        }
        public ObservableCollection<Account> Accounts { get; private set; }

        public async Task Initialize()
        {
            foreach (var item in Accounts)
                item.Dispose();
            Accounts.Clear();
            foreach (var item in await PlatformClient.Factory.ImportFromIE().ConfigureAwait(false))
            {
                var account = new Account(item);
                Accounts.Add(account);
            }
        }
        public void Dispose()
        {
            foreach (var item in Accounts)
                item.Dispose();
        }
    }
}