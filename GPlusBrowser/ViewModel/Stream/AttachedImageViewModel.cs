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

    public class AttachedImageViewModel : ViewModelBase
    {
        public AttachedImageViewModel(AttachedImage attachedAlbumModel)
        {
            _attachedAlbumModel = attachedAlbumModel;
            Initialize();
        }
        BitmapImage _image;
        AttachedImage _attachedAlbumModel;

        public BitmapImage Image
        {
            get { return _image; }
            set { Set(() => Image, ref _image, value); }
        }
        public async void Initialize()
        {
            Image = await DataCacheDictionary.DownloadImage(
                new Uri(_attachedAlbumModel.Image.ImageUrl.Replace("$SIZE_SEGMENT", "w640-h480")));
        }
    }
}
