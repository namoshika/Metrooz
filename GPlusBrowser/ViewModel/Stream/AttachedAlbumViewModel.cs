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

namespace GPlusBrowser.ViewModel
{
    using SunokoLibrary.Web.GooglePlus;
    using SunokoLibrary.Web.GooglePlus.Primitive;

    public class AttachedAlbumViewModel : AttachedContentViewModel
    {
        public AttachedAlbumViewModel(string title, AttachedAlbum attachedAlbumModel, AttachedImageViewModel[] thumbnailImages, ImageSource[] largeImages)
        {
            _title = title;
            _largeImages = largeImages;
            _thumbnailImages = thumbnailImages;
            _attachedAlbumModel = attachedAlbumModel;
            _selectedImageIndex = largeImages.Length > 0 ? 0 : -1;
            _selectedImage = _selectedImageIndex > -1 ? _largeImages[_selectedImageIndex] : null;
            _linkUrl = attachedAlbumModel.LinkUrl;
        }
        int _selectedImageIndex;
        string _title;
        Uri _linkUrl;
        ImageSource _selectedImage;
        ImageSource[] _largeImages;
        AttachedImageViewModel[] _thumbnailImages;
        AttachedAlbum _attachedAlbumModel;

        public int SelectedImageIndex
        {
            get { return _selectedImageIndex; }
            set
            {
                Set(() => SelectedImageIndex, ref _selectedImageIndex, value);
                SelectedImage = _selectedImageIndex > -1 ? _largeImages[value] : null;
            }
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
        public ImageSource SelectedImage
        {
            get { return _selectedImage; }
            set { Set(() => SelectedImage, ref _selectedImage, value); }
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
            var largeImgs = new List<ImageSource>();
            var downDatas = await Task.Factory.ContinueWhenAll(attachedAlbumModel.Pictures
                .SelectMany(imgInf =>
                    new[]{
                        new { IsThumbnail = true, Info = imgInf, Url = new Uri(imgInf.Image.ImageUrl.Replace("$SIZE_SEGMENT", "s70-c-k")) },
                        new { IsThumbnail = false, Info = imgInf, Url = new Uri(imgInf.Image.ImageUrl.Replace("$SIZE_SEGMENT", "w640-h480")) }
                    })
                .Select(async jobInf =>
                    new
                    {
                        IsThumbnail = jobInf.IsThumbnail,
                        Info = jobInf.Info,
                        Data = await DataCacheDictionary.DownloadImage(jobInf.Url).ConfigureAwait(false)
                    })
                .ToArray(), tsks => tsks.Select(tsk => tsk.Result)).ConfigureAwait(false);
            foreach (var jobInf in downDatas)
            {
                if (jobInf.IsThumbnail)
                    thumbImgs.Add(new AttachedImageViewModel(jobInf.Info, jobInf.Data));
                else
                    largeImgs.Add(jobInf.Data);
            }
            return new AttachedAlbumViewModel(title, attachedAlbumModel, thumbImgs.ToArray(), largeImgs.ToArray());
        }
    }
}
