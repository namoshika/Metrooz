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
            _image = image;
        }
        BitmapImage _image;
        AttachedImage _attachedAlbumModel;

        public BitmapImage Image
        {
            get { return _image; }
            set { Set(() => Image, ref _image, value); }
        }
        public static async Task<AttachedImageViewModel> Create(AttachedImage attachedAlbumModel)
        {
            var img = await DataCacheDictionary.DownloadImage(
                new Uri(attachedAlbumModel.Image.ImageUrl.Replace("$SIZE_SEGMENT", "w640-h480")));
            return new AttachedImageViewModel(attachedAlbumModel, img);
        }
    }
}
