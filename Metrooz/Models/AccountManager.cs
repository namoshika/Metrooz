﻿using SunokoLibrary.Application.Browsers;
using SunokoLibrary.Web.GooglePlus;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrooz.Models
{
    public class AccountManager
    {
        public AccountManager() { Accounts = new ObservableCollection<Account>(); }
        public ObservableCollection<Account> Accounts { get; private set; }
        public async Task<bool> Activate()
        {
            await Task.WhenAll(Accounts.Select(item => item.Deactivate()).ToArray());
            Accounts.Clear();

            //var cookieGetter = new IEBrowserManager().CreateIEPMCookieGetter();
            var cookieGetter = new GoogleChromeBrowserManager().CreateCookieImporters().First();
            if (cookieGetter.IsAvailable == false)
                return false;

            IPlatformClientBuilder[] builders;
            try { builders = await PlatformClient.Factory.ImportFrom(cookieGetter); }
            catch (FailToOperationException) { return false; }
            foreach (var item in builders)
            {
                var account = new Account(item);
                Accounts.Add(account);
            }
            return true;
        }

        public static readonly AccountManager Current = new AccountManager();
    }
}