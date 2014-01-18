using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace GPlusBrowser.Model
{
    public class SettingModel
    {
        public SettingModel()
        {
            _cookieFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            _settingDirectory = new System.IO.FileInfo(
                System.Configuration.ConfigurationManager.OpenExeConfiguration(
                System.Configuration.ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath).Directory;
        }
        System.IO.DirectoryInfo _settingDirectory;
        System.Runtime.Serialization.Formatters.Binary.BinaryFormatter _cookieFormatter;
        System.Net.CookieContainer _cookies;
        string _mailAddress;
        string _userName;
        string _userIconUrl;

        [System.Xml.Serialization.XmlIgnore]
        public SettingStatus IsSaved { get; private set; }
        [System.Xml.Serialization.XmlIgnore]
        public System.Net.CookieContainer Cookies
        {
            get { return _cookies; }
            set
            {
                _cookies = value;
                IsSaved = SettingStatus.NotSaved;
            }
        }
        public string MailAddress
        {
            get { return _mailAddress; }
            set
            {
                _mailAddress = value;
                IsSaved = SettingStatus.NotSaved;
            }
        }
        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                IsSaved = SettingStatus.NotSaved;
            }
        }
        public string UserIconUrl
        {
            get { return _userIconUrl; }
            set
            {
                _userIconUrl = value;
                IsSaved = SettingStatus.NotSaved;
            }
        }

        public void Save()
        {
            try
            {
                SerializeCookie();
                IsSaved = SettingStatus.Saved;
            }
            catch (Exception e)
            {
                IsSaved = SettingStatus.Error;

                var ex = new Exception("設定の保存に失敗しました。", e);
                ex.Data.Add("MailAddress", MailAddress);
                ex.Data.Add("SettingDirectory", _settingDirectory);
                throw ex;
            }
        }
        public void Load()
        {
            try
            {
                DeserializeCookie();
                IsSaved = SettingStatus.Saved;
            }
            catch (Exception e)
            {
                IsSaved = SettingStatus.Error;

                var ex = new Exception("設定の読み込みに失敗しました。", e);
                ex.Data.Add("MailAddress", MailAddress);
                ex.Data.Add("SettingDirectory", _settingDirectory);
                throw ex;
            }
        }
        void SerializeCookie()
        {
            if (string.IsNullOrEmpty(MailAddress))
                return;

            var cookiePath = new System.IO.FileInfo(
                string.Format("{0}\\{1}.cookie", _settingDirectory.FullName,
                Convert.ToString(MailAddress.GetHashCode(), 16)));
            cookiePath.Directory.Create();
            using (var strm = cookiePath.Open(System.IO.FileMode.Create, System.IO.FileAccess.Write))
                _cookieFormatter.Serialize(strm, Cookies);
        }
        void DeserializeCookie()
        {
            var cookiePath = new System.IO.FileInfo(
                string.Format("{0}\\{1}.cookie", _settingDirectory.FullName,
                Convert.ToString(MailAddress.GetHashCode(), 16)));
            if (cookiePath.Exists)
                using (var strm = cookiePath.OpenRead())
                    Cookies = (System.Net.CookieContainer)_cookieFormatter.Deserialize(strm);
            else
                Cookies = new System.Net.CookieContainer();
        }
    }
    public enum SettingStatus
    { NotSaved, Saved, Error, }
}