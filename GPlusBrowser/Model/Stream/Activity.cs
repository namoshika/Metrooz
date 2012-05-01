using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using SunokoLibrary.GooglePlus;

namespace GPlusBrowser.Model
{
    public class Activity : IDisposable
    {
        public Activity(ActivityInfo info)
        {
            Comments = new ObservableCollection<Comment>();
            _info = info;
            _info.Refreshed += _info_Refreshed;
            _commentObj = _info.GetComments(false, true).Subscribe(_info_comment_OnNext);
            Update(_info);
        }
        ActivityInfo _info;
        IDisposable _commentObj;

        public string Id { get; private set; }
        public Uri Url { get; private set; }
        public Uri OwnerIcon { get; private set; }
        public DateTime PostDate { get; private set; }
        public string OwnerName { get; private set; }
        public string Content { get; private set; }
        public PostStatusType PostStatus { get; private set; }
        public StyleElement ContentElement { get; private set; }
        public ObservableCollection<Comment> Comments { get; private set; }

        public async Task<bool> CommentPost(string content)
        {
            try
            {
                await _info.PostComment(content);
                return true;
            }
            catch (FailToOperationException)
            {
                if (System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Break();
                return false;
            }
        }
        public void Update(ActivityInfo newValue)
        {
            using (newValue.GetParseLocker())
            {
                Id = newValue.Id;
                PostStatus = newValue.PostStatus;
                if (newValue.PostStatus != PostStatusType.Removed)
                {
                    OwnerIcon = new Uri(newValue.PostUser.IconImageUrlText.Replace("$SIZE_SEGMENT", "s25-c-k"));
                    OwnerName = newValue.PostUser.Name;
                    PostDate = newValue.PostDate;
                    Content = newValue.Text;
                    ContentElement = newValue.ParsedContent;
                    Url = newValue.PostUrl;
                }
            }
            OnUpdated(new EventArgs());
        }
        public void Dispose()
        {
            _commentObj.Dispose();
            Comments.Clear();
        }
        void _info_Refreshed(object sender, EventArgs e) { Update(_info); }
        void _info_comment_OnNext(CommentInfo comment)
        {
            switch (comment.Status)
            {
                case PostStatusType.Removed:
                    var item = Comments.FirstOrDefault(cmme => cmme.CommentInfo.Id == comment.Id);
                    if (item != null)
                        Comments.Remove(item);
                    break;
                case PostStatusType.Edited:
                    item = Comments.FirstOrDefault(comme => comme.CommentInfo.Id == comment.Id);
                    if (item != null)
                        item.Refresh(comment);
                    else
                        goto default;
                    break;
                default:
                    var idx = Comments.Count - 1;
                    for (; idx >= 0 && Comments[idx].CommentInfo.CommentDate > comment.CommentDate; idx--) ;
                    Comments.Insert(idx + 1, new Comment(comment));
                    break;
            }
        }

        public event EventHandler Updated;
        protected virtual void OnUpdated(EventArgs e)
        {
            if (Updated != null)
                Updated(this, e);
        }
    }
}
