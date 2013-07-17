﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Threading;

namespace GPlusBrowser.ViewModel
{
    using Model;

    public class CommentViewModel : ViewModelBase, IDisposable
    {
        public CommentViewModel(Comment model, AccountViewModel topLevel, Dispatcher uiThreadDispatcher)
            : base(uiThreadDispatcher, topLevel)
        {
            _model = model;
            Id = model.CommentInfo.Id;
            model_Refreshed(null, null);
            model.Refreshed += model_Refreshed;
        }
        Comment _model;
        ImageSource _ownerIconUrl;
        string _id;
        string _ownerName;
        string _commentContent;
        string _commentDate;
        System.Windows.Documents.Inline _postContentInline;

        public string Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Id"));
            }
        }
        public string CommentContent
        {
            get { return _commentContent; }
            set
            {
                _commentContent = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("CommentContent"));
            }
        }
        public string OwnerName
        {
            get { return _ownerName; }
            set
            {
                _ownerName = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("OwnerName"));
            }
        }
        public string CommentDate
        {
            get { return _commentDate; }
            set
            {
                _commentDate = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("CommentDate"));
            }
        }
        public ImageSource OwnerIconUrl
        {
            get { return _ownerIconUrl; }
            set
            {
                _ownerIconUrl = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("OwnerIconUrl"));
            }
        }
        public System.Windows.Documents.Inline PostContentInline
        {
            get { return _postContentInline; }
            set
            {
                _postContentInline = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("PostContentInline"));
            }
        }
        public void Dispose()
        {
            if (_model != null)
                _model.Refreshed -= model_Refreshed;
            _id = null;
            _ownerName = null;
            _commentContent = null;
            _commentDate = null;
            _postContentInline = null;
        }

        void model_Refreshed(object sender, EventArgs e)
        {
            CommentDate = _model.CommentInfo.CommentDate >= DateTime.Today
                ? _model.CommentInfo.CommentDate.ToString("HH:mm")
                : _model.CommentInfo.CommentDate.ToString("yyyy/MM/dd");
            OwnerName = _model.CommentInfo.Owner.Name;
            //OwnerIconUrl = await DataCacheDictionary.DownloadImage(new Uri(_model.CommentInfo.Owner.IconImageUrlText
            //    .Replace("$SIZE_SEGMENT", "s25-c-k")));

            var element = _model.CommentInfo.ParsedContent;
            UiThreadDispatcher.InvokeAsync(() => PostContentInline = ActivityViewModel.PrivateConvertInlines(element));
        }
    }
}