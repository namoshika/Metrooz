using SunokoLibrary.Collection.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPlusBrowser
{
    public static class DataCacheDictionary
    {
        static DataCacheDictionary()
        {
            _cacheDictionary = new Dictionary<Uri, Uri>();
        }
        static Dictionary<Uri, Uri> _cacheDictionary;

        public static Task<Uri> DownloadUserIcon(Uri dataUrl)
        {
            Uri tmp;
            if (_cacheDictionary.TryGetValue(dataUrl, out tmp))
                return Task.FromResult(tmp);
            else
                return Task.Factory.StartNew(() =>
                    {
                        var req = System.Net.HttpWebRequest.Create(dataUrl);
                        var res = (System.Net.HttpWebResponse)req.GetResponse();
                        var cacheFilePath = System.IO.Path.GetTempFileName();
                        int recieveByte;
                        byte[] buff = new byte[1024];
                        using (var nstrm = new System.IO.BufferedStream(res.GetResponseStream()))
                        using (var fstrm = System.IO.File.OpenWrite(cacheFilePath))
                            while ((recieveByte = nstrm.Read(buff, 0, buff.Length)) > 0)
                                fstrm.Write(buff, 0, recieveByte);
                        return new Uri(cacheFilePath);
                    });
        }
        public static void Clear()
        {
            foreach (var item in _cacheDictionary)
                System.IO.File.Delete(item.Value.AbsolutePath);
        }
    }
}
