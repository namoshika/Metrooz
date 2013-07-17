using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace GPlusBrowser
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            var iconStream = Application.GetResourceStream(
                new Uri("pack://application:,,,/GPlusBrowser;component/Resources/TrayIcon.ico")).Stream;
            _trayIcon = new System.Windows.Forms.NotifyIcon();
            _trayIcon.Icon = new System.Drawing.Icon(iconStream);
            _trayIcon.ContextMenu = new System.Windows.Forms.ContextMenu(
                new[] { new System.Windows.Forms.MenuItem("終了", (sender, e) => App.Current.Shutdown()) });
            _trayIcon.MouseDoubleClick += _trayIcon_MouseDoubleClick;
            _trayIcon.Visible = true;
            _settingManager = new Model.SettingModelManager();
            _accountManager = new Model.AccountManager();
            _accountSwitcherVM = new ViewModel.PageSwitcherViewModel(_accountManager, Dispatcher);
            _notificationControl = new Controls.NotificationControl();
            Exit += App_Exit;
            App.Current.ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;

            _accountManager.Initialize();

            //テストコード
            var btnA = new System.Windows.Controls.Button() { Height = 100, Content = "Add", };
            btnA.Click += btnA_Click;
            var btnB = new System.Windows.Controls.Button() { Height = 100, Content = "Remove", };
            btnB.Click += btnB_Click;

            //_notificationControl.Children.Add(new FrameworkElement() { Height = 400 });
            //_notificationControl.Children.Add(new FrameworkElement() { Height = 200 });
            //_notificationControl.Children.Add(btnA);
            //_notificationControl.Children.Add(btnB);
        }
        MainWindow _mainWindow;
        Controls.NotificationControl _notificationControl;
        System.Windows.Forms.NotifyIcon _trayIcon;
        ViewModel.PageSwitcherViewModel _accountSwitcherVM;
        Model.SettingModelManager _settingManager;
        Model.AccountManager _accountManager;

        void _trayIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //_notificationControl.Children.Add(new FrameworkElement() { Height = 200 });
            if (_mainWindow != null)
                _mainWindow.Activate();
            else
            {
                _mainWindow = new MainWindow();
                _mainWindow.Closing += _mainWindow_Closing;
                _mainWindow.Loaded += _mainWindow_Loaded;
                _mainWindow.Show();
            }
        }
        void _mainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _mainWindow.DataContext = _accountSwitcherVM;
            _mainWindow.UpdateLayout();
        }
        void _mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _mainWindow.Closing -= _mainWindow_Closing;
            _mainWindow.Loaded -= _mainWindow_Loaded;
            _mainWindow = null;
        }
        void btnA_Click(object sender, RoutedEventArgs e)
        {
            //_notificationControl.Children.Add(new FrameworkElement() { Height = 200 });
        }
        void btnB_Click(object sender, RoutedEventArgs e)
        {
            //_notificationControl.Children.RemoveAt(0);
        }
        void App_Exit(object sender, ExitEventArgs e)
        {
            DataCacheDictionary.Clear();
            _accountSwitcherVM.Dispose();
            _accountManager.Dispose();
            _trayIcon.Dispose();
        }
    }
}
