using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace GPlusBrowser.Model
{
    public class SettingModelManager
    {
        public SettingModelManager()
        {
            Items = new List<SettingModel>();
        }

        public List<SettingModel> Items { get; private set; }
        public System.Threading.Tasks.Task Save()
        {
            return System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    Properties.Settings.Default.AccountSettings = Items.ToArray();
                    Properties.Settings.Default.Save();
                    foreach (var item in Items)
                        item.Save();
                });
        }
        public System.Threading.Tasks.Task Reload()
        {
            return System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    Properties.Settings.Default.Reload();
                    Items.Clear();
                    if (Properties.Settings.Default.AccountSettings != null)
                    {
                        Items.AddRange(Properties.Settings.Default.AccountSettings);
                        foreach (var item in Items)
                            item.Load();
                    }
                });
        }
    }
}
