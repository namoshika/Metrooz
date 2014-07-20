using Livet;
using Livet.Commands;
using SunokoLibrary.Web.GooglePlus;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Media;
namespace Metrooz.ViewModels
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
            set
            {
                _width = value;
                RaisePropertyChanged(() => Width);
            }
        }
        public double Height
        {
            get { return _height; }
            set
            {
                _height = value;
                RaisePropertyChanged(() => Height);
            }
        }
        public Uri LinkUrl
        {
            get { return _linkUrl; }
            set
            {
                _linkUrl = value;
                RaisePropertyChanged(() => LinkUrl);
            }
        }
        public ImageSource Image
        {
            get { return _image; }
            set
            {
                _image = value;
                RaisePropertyChanged(() => Image);
            }
        }
        public static async Task<AttachedImageViewModel> Create(AttachedImage attachedAlbumModel, string option = "w640-h500")
        {
            var img = await DataCacheDictionary.DownloadImage(
                new Uri(attachedAlbumModel.Image.ImageUrl.Replace("$SIZE_SEGMENT", option))).ConfigureAwait(false);
            return img != null ? new AttachedImageViewModel(attachedAlbumModel, img) : null;
        }
    }
}
