using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GPlusBrowser.Controls
{
    /// <summary>
    /// CommentListBox.xaml の相互作用ロジック
    /// </summary>
    public partial class CommentListBox : UserControl
    {
        public CommentListBox()
        {
            InitializeComponent();
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }
        DateTime _backDate;
        public bool IsExpand
        {
            get { return (bool)GetValue(IsExpandProperty); }
            set { SetValue(IsExpandProperty, value); }
        }
        public System.Collections.IEnumerable Comments
        {
            get { return (System.Collections.IEnumerable)GetValue(CommentsProperty); }
            set { SetValue(CommentsProperty, value); }
        }

        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            var nowDate = DateTime.UtcNow;
            var span = nowDate - _backDate;
            double height;

            if (itemContainer.Items.Count == 0)
            {
                height = 0;
            }
            else
            {
                var container = (FrameworkElement)itemContainer.ItemContainerGenerator
                    .ContainerFromIndex(itemContainer.Items.Count - 1);
                height = container.ActualHeight;
            }
        }

        public static readonly DependencyProperty IsExpandProperty = DependencyProperty.Register(
            "IsExpand", typeof(bool), typeof(CommentListBox), new UIPropertyMetadata(false));
        public static readonly DependencyProperty CommentsProperty = DependencyProperty.Register(
            "Comments", typeof(System.Collections.IEnumerable), typeof(CommentListBox));
    }
}
