using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace GPlusBrowser.Model
{
    [Serializable]
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

        public string MailAddress { get; set; }
        public string UserName { get; set; }
        public string UserIconUrl { get; set; }
        [System.Xml.Serialization.XmlIgnore]
        public System.Net.CookieContainer Cookies { get; set; }

        public void Save() { SerializeCookie(); }
        public void Load() { DeserializeCookie(); }
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
}