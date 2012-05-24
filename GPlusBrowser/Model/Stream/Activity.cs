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
            ActivityInfo = info;
            ActivityInfo.Refreshed += _info_Refreshed;
            _commentObj = ActivityInfo.GetComments(false, true).Subscribe(_info_comment_OnNext);
        }
        IDisposable _commentObj;

        public ActivityInfo ActivityInfo { get; private set; }
        public ObservableCollection<Comment> Comments { get; private set; }

        public async Task<bool> CommentPost(string content)
        {
            try
            {
                await ActivityInfo.PostComment(content).ConfigureAwait(false);
                return true;
            }
            catch (FailToOperationException)
            {
                if (System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Break();
                return false;
            }
        }
        public void Dispose()
        {
            _commentObj.Dispose();
            Comments.Clear();
        }
        void _info_Refreshed(object sender, EventArgs e) { OnUpdated(new EventArgs()); }
        void _info_comment_OnNext(CommentInfo comment)
        {
            lock(Comments)
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