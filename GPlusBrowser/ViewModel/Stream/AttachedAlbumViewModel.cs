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

    public class AttachedAlbumViewModel : ViewModelBase
    {
        public AttachedAlbumViewModel(AttachedAlbum attachedAlbumModel, AccountViewModel topLevel, Dispatcher uiThreadDispatcher)
            : base(uiThreadDispatcher, topLevel)
        {
            _selectedImageIndex = 0;
            _attachedAlbumModel = attachedAlbumModel;

            _thumbnailImages = new BitmapImage[_attachedAlbumModel.Pictures.Length];
            _mainImages = new BitmapImage[_attachedAlbumModel.Pictures.Length];
            System.Threading.Tasks.Parallel
                .ForEach(
                _attachedAlbumModel.Pictures.Select((info, idx) => new { Info = info, Index = idx }),
                async pair =>
                {
                    _thumbnailImages[pair.Index] = await topLevel.DataCacheDict.DownloadImage(new Uri(pair.Info.ImageUrlText.Replace("$SIZE_SEGMENT", "s50-c-k"))).ConfigureAwait(false);
                    _mainImages[pair.Index] = await topLevel.DataCacheDict.DownloadImage(new Uri(pair.Info.ImageUrlText.Replace("$SIZE_SEGMENT", "w640-h480"))).ConfigureAwait(false);
                    if(pair.Index == _selectedImageIndex)
                        OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("SelectedImage"));
                });
        }
        int _selectedImageIndex;
        BitmapImage[] _thumbnailImages, _mainImages;
        AttachedAlbum _attachedAlbumModel;

        public int SelectedImageIndex
        {
            get { return _selectedImageIndex; }
            set
            {
                _selectedImageIndex = value;
                OnPropertyChanged(new System.ComponentModel
                    .PropertyChangedEventArgs("SelectedImageIndex"));
                OnPropertyChanged(new System.ComponentModel
                    .PropertyChangedEventArgs("SelectedImage"));
            }
        }
        public BitmapImage SelectedImage
        { get { return _mainImages[_selectedImageIndex]; } }
        public BitmapImage[] ThumbnailImages { get { return _thumbnailImages; } }
    }
}
