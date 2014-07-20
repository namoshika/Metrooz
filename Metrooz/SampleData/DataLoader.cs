using SunokoLibrary.Collections.Generic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Metrooz.SampleData
{
    public static class DataLoader
    {
        static DataLoader()
        {
            sampleFileDir = "SampleData/Files/";
        }
        static readonly string sampleFileDir;
        static readonly CacheDictionary<string, ImageCache, Task<BitmapImage>> _imgCacheDictionary = new CacheDictionary<string, ImageCache, Task<BitmapImage>>(30, 10, false, tsk => new ImageCache() { Value = tsk });
        public static Task<BitmapImage> LoadImage(string targetName)
        {
            if (targetName == null)
                return Task.FromResult((BitmapImage)null);
            return _imgCacheDictionary.Update(null, targetName, () => PrivateLoadImage(targetName)).Value;
        }
        static Task<BitmapImage> PrivateLoadImage(string targetName)
        {
            return Task.Run(async () =>
            {
                try
                {
                    BitmapImage img;
                    using (var mStrm = new MemoryStream())
                    {
                        int recieveByte;
                        byte[] buff = new byte[1024];
                        using (var strm = new FileStream(sampleFileDir + targetName, FileMode.Open, FileAccess.Read))
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

                        //メモリーリーク対策として駆動スレッドのDispatcherを毎回破棄するようにする
                        //参考資料:
                        // [バックグラウンド スレッドで UI 要素を作るとメモリリークする (WPF)]
                        // http://grabacr.net/archives/1851
                        Dispatcher.CurrentDispatcher.InvokeShutdown();

                        return img;
                    }
                }
                catch (System.Net.Http.HttpRequestException) { return null; }
                catch (NotSupportedException) { return null; }
            });
        }

        class ImageCache : ICacheInfo<Task<BitmapImage>>
        { public Task<BitmapImage> Value { get; set; } }
    }
}
