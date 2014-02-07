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

    public class AttachedAlbumViewModel : ViewModelBase
    {
        public AttachedAlbumViewModel(AttachedAlbum attachedAlbumModel, BitmapImage[] thumbnailImages, BitmapImage[] largeImages)
        {
            _largeImages = largeImages;
            _thumbnailImages = thumbnailImages;
            _attachedAlbumModel = attachedAlbumModel;
            SelectedImageIndex = largeImages.Length > 0 ? 0 : -1;
        }
        int _selectedImageIndex;
        BitmapImage _selectedImage;
        BitmapImage[] _thumbnailImages, _largeImages;
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
        public BitmapImage SelectedImage
        {
            get { return _selectedImage; }
            set { Set(() => SelectedImage, ref _selectedImage, value); }
        }
        public BitmapImage[] ThumbnailImages
        {
            get { return _thumbnailImages; }
            set { Set(() => ThumbnailImages, ref _thumbnailImages, value); }
        }
        public static async Task<AttachedAlbumViewModel> Create(AttachedAlbum attachedAlbumModel)
        {
            var thumbImgs = new List<BitmapImage>();
            var largeImgs = new List<BitmapImage>();
            var downDatas = await Task.Factory.ContinueWhenAll(attachedAlbumModel.Pictures
                .SelectMany(imgInf =>
                    new[]{
                        new { IsThumbnail = true, Url = new Uri(imgInf.ImageUrl.Replace("$SIZE_SEGMENT", "s50-c-k")) },
                        new { IsThumbnail = false, Url = new Uri(imgInf.ImageUrl.Replace("$SIZE_SEGMENT", "w640-h480")) }
                    })
                .Select(async jobInf =>
                    new
                    {
                        IsThumbnail = jobInf.IsThumbnail,
                        Data = await DataCacheDictionary.DownloadImage(jobInf.Url).ConfigureAwait(false)
                    })
                .ToArray(), tsks => tsks.Select(tsk => tsk.Result));
            foreach (var jobInf in downDatas)
            {
                if (jobInf.IsThumbnail)
                    thumbImgs.Add(jobInf.Data);
                else
                    largeImgs.Add(jobInf.Data);
            }
            return new AttachedAlbumViewModel(attachedAlbumModel, thumbImgs.ToArray(), largeImgs.ToArray());
        }
    }
}
