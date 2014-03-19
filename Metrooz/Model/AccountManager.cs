using SunokoLibrary.Web.GooglePlus;
using SunokoLibrary.Web.GooglePlus.Primitive;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrooz.Model
{
    public class AccountManager
    {
        public AccountManager()
        {
            Accounts = new ObservableCollection<Account>();
        }
        public ObservableCollection<Account> Accounts { get; private set; }

        public async Task Initialize()
        {
            await Task.WhenAll(Accounts.Select(item => item.Deactivate()).ToArray());
            Accounts.Clear();
            foreach (var item in await PlatformClient.Factory.ImportFromIE().ConfigureAwait(false))
            {
                var account = new Account(item);
                Accounts.Add(account);
            }
            OnInitialized(new EventArgs());
        }
        public event EventHandler Initialized;
        protected virtual void OnInitialized(EventArgs e)
        {
            if (Initialized != null)
                Initialized(this, e);
        }
    }
}