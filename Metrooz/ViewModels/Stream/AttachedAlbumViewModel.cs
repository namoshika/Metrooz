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
    public class AttachedAlbumViewModel : AttachedContentViewModel
    {
        public AttachedAlbumViewModel(string title, AttachedAlbum attachedAlbumModel, AttachedImageViewModel[] thumbnailImages, ImageSource largeImage)
        {
            _title = title;
            _largeImage = largeImage;
            _thumbnailImages = thumbnailImages;
            _attachedAlbumModel = attachedAlbumModel;
            _linkUrl = attachedAlbumModel.LinkUrl;
            _width = largeImage.Width;
            _height = largeImage.Height;
        }
        double _width, _height;
        string _title;
        Uri _linkUrl;
        ImageSource _largeImage;
        AttachedImageViewModel[] _thumbnailImages;
        AttachedAlbum _attachedAlbumModel;

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
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                RaisePropertyChanged(() => Title);
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
        public ImageSource LargeImage
        {
            get { return _largeImage; }
            set
            {
                _largeImage = value;
                RaisePropertyChanged(() => LargeImage);
            }
        }
        public AttachedImageViewModel[] ThumbnailImages
        {
            get { return _thumbnailImages; }
            set
            {
                _thumbnailImages = value;
                RaisePropertyChanged(() => ThumbnailImages);
            }
        }
        public static async Task<AttachedAlbumViewModel> Create(AttachedAlbum attachedAlbumModel)
        {
            var title = attachedAlbumModel.Album.Name;
            var thumbImgs = new List<AttachedImageViewModel>();
            var downDatas = await Task.WhenAll(attachedAlbumModel.Pictures
                .Select(async imgInf => new
                {
                    Info = imgInf,
                    Data = await DataCacheDictionary.DownloadImage(new Uri(imgInf.Image.ImageUrl.Replace("$SIZE_SEGMENT", "s70"))).ConfigureAwait(false)
                }))
                .ConfigureAwait(false);
            var largeImg = await DataCacheDictionary.DownloadImage(new Uri(attachedAlbumModel.Pictures.First().Image.ImageUrl.Replace("$SIZE_SEGMENT", "w640-h480")));
            foreach (var jobInf in downDatas)
                thumbImgs.Add(new AttachedImageViewModel(jobInf.Info, jobInf.Data));
            return new AttachedAlbumViewModel(title, attachedAlbumModel, thumbImgs.ToArray(), largeImg);
        }
    }
}
