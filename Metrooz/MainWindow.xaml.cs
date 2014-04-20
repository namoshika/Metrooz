using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SunokoLibrary.Web.GooglePlus;

namespace Metrooz
{
    using Metrooz.ViewModel;

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Messenger.Default.Register<DialogOptionInfo>(this, Recieved_DialogMessage);
        }

        protected override void OnClosed(EventArgs e)
        {
            ViewModel.ViewModelLocator.Cleanup();
            base.OnClosed(e);
        }
        void Recieved_DialogMessage(DialogOptionInfo message)
        {
            Dispatcher.InvokeAsync(async () =>
                {
                    var res = await this.ShowMessageAsync(message.Title, message.Message, message.Style, message.Settings);
                    message.CallbackTaskSource.SetResult(res);
                });
        }
    }
}
