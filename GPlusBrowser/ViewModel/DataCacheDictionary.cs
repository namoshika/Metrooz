using SunokoLibrary.Collection.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GPlusBrowser
{
    public static class DataCacheDictionary
    {
        static DataCacheDictionary() { System.Net.ServicePointManager.DefaultConnectionLimit = 8; }
        static DateTime _lastCleanupTime = DateTime.UtcNow;
        static readonly HttpClient _defaultHttpClient = new HttpClient();
        static readonly Dictionary<Uri, WeakReference<BitmapImage>> _imgCacheDictionary = new Dictionary<Uri, WeakReference<BitmapImage>>();
        static readonly Dictionary<Uri, Task<BitmapImage>> _imgJobDictionary = new Dictionary<Uri, Task<BitmapImage>>();
        static readonly System.Threading.SemaphoreSlim _syncer = new System.Threading.SemaphoreSlim(1, 1);

        public async static Task<BitmapImage> DownloadImage(Uri imageUrl, HttpClient client = null)
        {
            if (imageUrl == null)
                return null;
            try
            {
                await _syncer.WaitAsync();

                Task<BitmapImage> aa;
                if (_imgJobDictionary.TryGetValue(imageUrl, out aa))
                    return await aa;
                else
                    return await Task.Run(async () =>
                    {
                        try
                        {
                            WeakReference<BitmapImage> imgRef;
                            BitmapImage img;
                            bool isNewItem = true;

                            if (_imgCacheDictionary.TryGetValue(imageUrl, out imgRef) == false || (isNewItem = imgRef.TryGetTarget(out img)) == false)
                                using (var mStrm = new System.IO.MemoryStream())
                                {
                                    client = client ?? _defaultHttpClient;
                                    int recieveByte;
                                    byte[] buff = new byte[1024];
                                    using (var strm = await client.GetStreamAsync(imageUrl))
                                        while ((recieveByte = await strm.ReadAsync(buff, 0, buff.Length)) > 0)
                                            mStrm.Write(buff, 0, recieveByte);
                                    mStrm.Seek(0, System.IO.SeekOrigin.Begin);

                                    img = new BitmapImage();
                                    img.BeginInit();
                                    img.CacheOption = BitmapCacheOption.OnLoad;
                                    img.CreateOptions = BitmapCreateOptions.None;
                                    img.StreamSource = mStrm;
                                    img.EndInit();
                                    img.Freeze();

                                    if (isNewItem)
                                        _imgCacheDictionary.Add(imageUrl, new WeakReference<BitmapImage>(img));
                                    else
                                        imgRef.SetTarget(img);
                                }
                            if (DateTime.UtcNow - _lastCleanupTime > TimeSpan.FromMinutes(30))
                            {
                                _lastCleanupTime = DateTime.UtcNow;
                                GC();
                            }
                            return img;
                        }
                        catch (System.Net.Http.HttpRequestException) { return null; }
                        catch (NotSupportedException) { return null; }
                    });
            }
            finally
            { _syncer.Release(); }
        }
        public static void GC()
        {
            BitmapImage tmp;
            foreach (var item in _imgCacheDictionary.ToArray())
            {
                if (item.Value.TryGetTarget(out tmp) == false)
                    _imgCacheDictionary.Remove(item.Key);
            }
        }
    }
}
