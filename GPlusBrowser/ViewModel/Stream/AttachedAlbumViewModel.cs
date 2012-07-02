using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace GPlusBrowser.ViewModel
{
    using SunokoLibrary.GooglePlus;

    public class AttachedAlbumViewModel : ViewModelBase
    {
        public AttachedAlbumViewModel(AttachedAlbum attachedAlbumModel, Dispatcher uiThreadDispatcher)
            : base(uiThreadDispatcher)
        {
            _selectedImageIndex = 0;
            _attachedAlbumModel = attachedAlbumModel;
            _largeImageUrlArray = _attachedAlbumModel.Pictures
                .Select(info => new Uri(info.ImageUrlText.Replace("$SIZE_SEGMENT", "w640-h480")))
                .ToArray();
            _selectedImageUrl = _largeImageUrlArray[_selectedImageIndex];
            SmallImageUrlList = new ObservableCollection<Uri>(_attachedAlbumModel.Pictures
                .Select(info => new Uri(info.ImageUrlText.Replace("$SIZE_SEGMENT", "w640-h480"))));
        }
        int _selectedImageIndex;
        Uri _selectedImageUrl;
        Uri[] _largeImageUrlArray;
        AttachedAlbum _attachedAlbumModel;

        public int SelectedImageIndex
        {
            get { return _selectedImageIndex; }
            set
            {
                _selectedImageIndex = value;
                OnPropertyChanged(new System.ComponentModel
                    .PropertyChangedEventArgs("SelectedImageIndex"));
                SelectedImageUrl = _largeImageUrlArray[value];
            }
        }
        public Uri SelectedImageUrl
        {
            get { return _selectedImageUrl; }
            set
            {
                _selectedImageUrl = value;
                OnPropertyChanged(new System.ComponentModel
                    .PropertyChangedEventArgs("SelectedImageUrl"));
            }
        }
        public ObservableCollection<Uri> SmallImageUrlList { get; private set; }
    }
}
