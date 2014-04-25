using SunokoLibrary.Collection.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Metrooz
{
    public static class DataCacheDictionary
    {
        static DataCacheDictionary()
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = 16;
            _imgCacheDictionary.CacheOuted += _imgCacheDictionary_CacheOuted;
        }
        static DateTime _lastCleanupTime = DateTime.UtcNow;
        static readonly HttpClient _defaultHttpClient = new HttpClient();
        static readonly CacheDictionary<Uri, ImageCache, Task<BitmapImage>> _imgCacheDictionary = new CacheDictionary<Uri, ImageCache, Task<BitmapImage>>(90, 30, false, tsk => new ImageCache() { Value = tsk });

        public static Task<BitmapImage> DownloadImage(Uri imageUrl, HttpClient client = null)
        {
            if (imageUrl == null)
                return Task.FromResult((BitmapImage)null);
            client = client ?? _defaultHttpClient;
            return _imgCacheDictionary.Update(null, imageUrl, () => PrivateDownloadImage(imageUrl, client)).Value;
        }
        static Task<BitmapImage> PrivateDownloadImage(Uri imageUrl, HttpClient client = null)
        {
            return Task.Run(async () =>
                {
                    try
                    {
                        BitmapImage img;
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

                            return img;
                        }
                    }
                    catch (System.Net.Http.HttpRequestException) { return null; }
                    catch (NotSupportedException) { return null; }
                });
        }
        static void _imgCacheDictionary_CacheOuted(object sender, CacheoutEventArgs<Task<BitmapImage>> e)
        {
            switch(e.Value.Status)
            {
                case TaskStatus.RanToCompletion:
                case TaskStatus.Faulted:
                case TaskStatus.Canceled:
                    e.Value.Dispose();
                    break;
            }
        }
    }
    public class ImageCache : ICacheInfo<Task<BitmapImage>>
    { public Task<BitmapImage> Value { get; set; } }

}
