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
    public class ImageBox : ListBox
    {
        static ImageBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ImageBox), new FrameworkPropertyMetadata(typeof(ImageBox)));
        }

        public bool HasManyImages
        {
            get { return (bool)GetValue(HasManyImagesProperty); }
            protected set { SetValue(HasManyImagesPropertyKey, value); }
        }
        public int ItemCount
        {
            get { return (int)GetValue(ItemCountProperty); }
            protected set { SetValue(ItemCountPropertyKey, value); }
        }
        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }
        public object Content
        {
            get { return (object)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }
        public DataTemplate ContentTemplate
        {
            get { return (DataTemplate)GetValue(ContentTemplateProperty); }
            set { SetValue(ContentTemplateProperty, value); }
        }
        public double DexpandHeight
        {
            get { return (double)GetValue(DexpandHeightProperty); }
            protected set { SetValue(DexpandHeightPropertyKey, value); }
        }
        public ICommand ClickHeaderCommand
        {
            get { return (ICommand)GetValue(ClickHeaderCommandProperty); }
            set { SetValue(ClickHeaderCommandProperty, value); }
        }
        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ItemCount = Items.Count;
            if (SelectedIndex < 0 && Items.Count > 0)
                SelectedIndex = 0;
            base.OnItemsChanged(e);
        }

        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            "Content", typeof(object), typeof(ImageBox), new UIPropertyMetadata(null));
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            "Header", typeof(string), typeof(ImageBox), new UIPropertyMetadata(null));
        public static readonly DependencyProperty ContentTemplateProperty = DependencyProperty.Register(
            "ContentTemplate", typeof(DataTemplate), typeof(ImageBox), new UIPropertyMetadata(null));
        public static readonly DependencyProperty ClickHeaderCommandProperty = DependencyProperty.Register(
            "ClickHeaderCommand", typeof(ICommand), typeof(ImageBox), new UIPropertyMetadata(null));
        static readonly DependencyPropertyKey DexpandHeightPropertyKey = DependencyProperty.RegisterReadOnly(
            "DexpandHeight", typeof(double), typeof(ImageBox), new UIPropertyMetadata(33.0));
        static readonly DependencyPropertyKey HasManyImagesPropertyKey = DependencyProperty.RegisterReadOnly(
            "HasManyImages", typeof(bool), typeof(ImageBox), new UIPropertyMetadata(true));
        static readonly DependencyPropertyKey ItemCountPropertyKey = DependencyProperty.RegisterReadOnly(
            "ItemCount", typeof(int), typeof(ImageBox), new UIPropertyMetadata(0,
                (sender, e) =>
                    {
                        var result = (int)e.NewValue > 1;
                        ((ImageBox)sender).HasManyImages = result;
                        ((ImageBox)sender).DexpandHeight = result ? 60.0 : 33.0;
                    }));

        public static readonly DependencyProperty HasManyImagesProperty = HasManyImagesPropertyKey.DependencyProperty;
        public static readonly DependencyProperty ItemCountProperty = ItemCountPropertyKey.DependencyProperty;
        public static readonly DependencyProperty DexpandHeightProperty = DexpandHeightPropertyKey.DependencyProperty;
    }
}
