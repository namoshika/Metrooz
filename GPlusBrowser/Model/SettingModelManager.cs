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
        { Items = new List<SettingModel>(); }

        public List<SettingModel> Items { get; private set; }
        public void Save()
        {
            Properties.Settings.Default.AccountSettings = Items.ToArray();
            Properties.Settings.Default.Save();

            var exLst = new List<Exception>();
            foreach (var item in Items)
                try { item.Save(); }
                catch(Exception e)
                {
                    if (System.Diagnostics.Debugger.IsAttached)
                        exLst.Add(e);
                }
            if (exLst.Count > 0)
                throw new AggregateException("設定の保存中にエラーが発生しました。", exLst);
        }
        public void Reload()
        {
            Properties.Settings.Default.Reload();
            Items.Clear();
            if (Properties.Settings.Default.AccountSettings != null)
            {
                var exLst = new List<Exception>();
                Items.AddRange(Properties.Settings.Default.AccountSettings);
                foreach (var item in Items)
                    try { item.Load(); }
                    catch (Exception e)
                    {
                        if (System.Diagnostics.Debugger.IsAttached)
                            exLst.Add(e);
                    }
                if (exLst.Count > 0)
                    throw new AggregateException("設定の保存中にエラーが発生しました。", exLst);
            }
        }
    }
}