using SunokoLibrary.Web.GooglePlus;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hal.CookieGetterSharp;

namespace Metrooz.Model
{
    public class AccountManager
    {
        public AccountManager() { Accounts = new ObservableCollection<Account>(); }
        public ObservableCollection<Account> Accounts { get; private set; }

        public async Task<bool> Initialize()
        {
            await Task.WhenAll(Accounts.Select(item => item.Deactivate()).ToArray());
            Accounts.Clear();
            try
            {
                var cookieGetter = CookieGetter.CreateInstance(BrowserType.GoogleChrome);
                if (cookieGetter.Status.IsAvailable == false)
                    return false;
                foreach (var item in await PlatformClient.Factory.ImportFrom(cookieGetter).ConfigureAwait(false))
                {
                    var account = new Account(item);
                    Accounts.Add(account);
                }
                OnInitialized(new EventArgs());
                return true;
            }
            catch (FailToOperationException)
            {
                if (System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Break();
                return false;
            }
        }
        public event EventHandler Initialized;
        protected virtual void OnInitialized(EventArgs e)
        {
            if (Initialized != null)
                Initialized(this, e);
        }
    }
}