using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using SunokoLibrary.Web.GooglePlus;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Media;
namespace Metrooz.ViewModel
{
    public class AttachedImageViewModel : AttachedContentViewModel
    {
        public AttachedImageViewModel(AttachedImage attachedAlbumModel, ImageSource image)
        {
            _attachedAlbumModel = attachedAlbumModel;
            _linkUrl = attachedAlbumModel.LinkUrl;
            _image = image;
            _width = image.Width;
            _height = image.Height;
        }
        double _width, _height;
        Uri _linkUrl;
        ImageSource _image;
        AttachedImage _attachedAlbumModel;

        public double Width
        {
            get { return _width; }
            set { Set(() => Width, ref _width, value); }
        }
        public double Height
        {
            get { return _height; }
            set { Set(() => Height, ref _height, value); }
        }
        public Uri LinkUrl
        {
            get { return _linkUrl; }
            set { Set(() => LinkUrl, ref _linkUrl, value); }
        }
        public ImageSource Image
        {
            get { return _image; }
            set { Set(() => Image, ref _image, value); }
        }
        public static async Task<AttachedImageViewModel> Create(AttachedImage attachedAlbumModel, string option = "w640-h500")
        {
            var img = await DataCacheDictionary.DownloadImage(
                new Uri(attachedAlbumModel.Image.ImageUrl.Replace("$SIZE_SEGMENT", option))).ConfigureAwait(false);
            return img != null ? new AttachedImageViewModel(attachedAlbumModel, img) : null;
        }
    }
}
