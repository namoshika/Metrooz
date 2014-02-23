using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
namespace GPlusBrowser.ViewModel
{
    using SunokoLibrary.Web.GooglePlus;
    using SunokoLibrary.Web.GooglePlus.Primitive;

    public class AttachedImageViewModel : AttachedContentViewModel
    {
        public AttachedImageViewModel(AttachedImage attachedAlbumModel, BitmapImage image)
        {
            _attachedAlbumModel = attachedAlbumModel;
            _linkUrl = attachedAlbumModel.LinkUrl;
            _image = image;
        }
        Uri _linkUrl;
        BitmapImage _image;
        AttachedImage _attachedAlbumModel;

        public Uri LinkUrl
        {
            get { return _linkUrl; }
            set { Set(() => LinkUrl, ref _linkUrl, value); }
        }
        public BitmapImage Image
        {
            get { return _image; }
            set { Set(() => Image, ref _image, value); }
        }
        public static async Task<AttachedImageViewModel> Create(AttachedImage attachedAlbumModel, string option = "w640-h500")
        {
            var img = await DataCacheDictionary.DownloadImage(
                new Uri(attachedAlbumModel.Image.ImageUrl.Replace("$SIZE_SEGMENT", option))).ConfigureAwait(false);
            return new AttachedImageViewModel(attachedAlbumModel, img);
        }
    }
}
