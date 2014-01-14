using SunokoLibrary.Collection.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GPlusBrowser
{
    public class DataCacheDictionary
    {
        public DataCacheDictionary() { _client = new System.Net.Http.HttpClient(); }
        public DataCacheDictionary(System.Net.Http.HttpClient client) { _client = client; }
        System.Net.Http.HttpClient _client;
        public async Task<Uri> DownloadData(Uri dataUrl)
        {
            Uri tmp;
            if (dataUrl == null)
                return null;
            if (_urlCacheDictionary.TryGetValue(dataUrl, out tmp))
                return tmp;
            else
            {
                var cacheFilePath = System.IO.Path.GetTempFileName();
                int recieveByte;
                byte[] buff = new byte[1024];
                using (var strm = await _client.GetStreamAsync(dataUrl).ConfigureAwait(false))
                using (var fstrm = System.IO.File.OpenWrite(cacheFilePath))
                    while ((recieveByte = strm.Read(buff, 0, buff.Length)) > 0)
                        fstrm.Write(buff, 0, recieveByte);
                return new Uri(cacheFilePath);
            }
        }
        public async Task<BitmapImage> DownloadImage(Uri imageUrl)
        {
            if (imageUrl == null)
                return null;
            try
            {
                var recievedFile = await DownloadData(imageUrl).ConfigureAwait(false);
                var imgCacheUrl = recievedFile;
                WeakReference<BitmapImage> imgRef;
                BitmapImage img;
                if (_imgCacheDictionary.TryGetValue(imgCacheUrl, out imgRef))
                {
                    if (imgRef.TryGetTarget(out img) == false)
                    {
                        img = new BitmapImage(imgCacheUrl) { CacheOption = BitmapCacheOption.OnLoad };
                        imgRef.SetTarget(img);
                    }
                }
                else
                {
                    img = new BitmapImage(imgCacheUrl) { CacheOption = BitmapCacheOption.OnLoad };
                    _imgCacheDictionary.Add(imgCacheUrl, new WeakReference<BitmapImage>(img));
                }
                if (img.IsFrozen == false)
                    img.Freeze();
                return img;
            }
            catch (System.Net.Http.HttpRequestException)
            {
                //System.Diagnostics.Debug.Assert(false, "DataCacheDictionary.DownloadImage()の結果、通信エラーが発生しました。");
                return null;
            }
        }

        static DataCacheDictionary() { System.Net.ServicePointManager.DefaultConnectionLimit = 16; }
        public readonly static DataCacheDictionary Default = new DataCacheDictionary();
        static readonly Dictionary<Uri, Uri> _urlCacheDictionary = new Dictionary<Uri, Uri>();
        static readonly Dictionary<Uri, WeakReference<BitmapImage>> _imgCacheDictionary = new Dictionary<Uri, WeakReference<BitmapImage>>();

        public static void Clear()
        {
            foreach (var item in _urlCacheDictionary)
                try { System.IO.File.Delete(item.Value.AbsolutePath); }
                catch (System.IO.IOException) { }
        }
    }
}
