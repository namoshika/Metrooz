using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
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
    using SunokoLibrary.Web.GooglePlus;
    using SunokoLibrary.Web.GooglePlus.Primitive;

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
            set { Set(() => Width, ref _width, value); }
        }
        public double Height
        {
            get { return _height; }
            set { Set(() => Height, ref _height, value); }
        }
        public string Title
        {
            get { return _title; }
            set { Set(() => Title, ref _title, value); }
        }
        public Uri LinkUrl
        {
            get { return _linkUrl; }
            set { Set(() => LinkUrl, ref _linkUrl, value); }
        }
        public ImageSource LargeImage
        {
            get { return _largeImage; }
            set { Set(() => LargeImage, ref _largeImage, value); }
        }
        public AttachedImageViewModel[] ThumbnailImages
        {
            get { return _thumbnailImages; }
            set { Set(() => ThumbnailImages, ref _thumbnailImages, value); }
        }
        public static async Task<AttachedAlbumViewModel> Create(AttachedAlbum attachedAlbumModel)
        {
            var title = attachedAlbumModel.Album.Name;
            var thumbImgs = new List<AttachedImageViewModel>();
            var downDatas = await Task.WhenAll(attachedAlbumModel.Pictures
                .Select(async imgInf => new {
                    Info = imgInf,
                    Data = await DataCacheDictionary.DownloadImage(new Uri(imgInf.Image.ImageUrl.Replace("$SIZE_SEGMENT", "s70"))).ConfigureAwait(false) }))
                .ConfigureAwait(false);
            var largeImg = await DataCacheDictionary.DownloadImage(new Uri(attachedAlbumModel.Pictures.First().Image.ImageUrl.Replace("$SIZE_SEGMENT", "w640-h480")));
            foreach (var jobInf in downDatas)
                thumbImgs.Add(new AttachedImageViewModel(jobInf.Info, jobInf.Data));
            return new AttachedAlbumViewModel(title, attachedAlbumModel, thumbImgs.ToArray(), largeImg);
        }
    }
}
