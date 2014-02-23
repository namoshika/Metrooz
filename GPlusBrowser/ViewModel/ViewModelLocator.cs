/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:GPlusBrowser"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;

namespace GPlusBrowser.ViewModel
{
    using GPlusBrowser.Model;

    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            if (ViewModelBase.IsInDesignModeStatic)
            {
                //// Create design time view services and models
                //SimpleIoc.Default.Register<DesignDataService>();
            }
            else
            {
                // Create run time view services and models
                SimpleIoc.Default.Register<AccountManager>();
            }
            SimpleIoc.Default.Register<MainViewModel>();
        }

        public MainViewModel Main
        { get { return ServiceLocator.Current.GetInstance<MainViewModel>(); } }
        
        public static void Cleanup()
        {
            // TODO Clear the ViewModels
            SimpleIoc.Default.GetInstance<MainViewModel>().Cleanup();
            SimpleIoc.Default.Unregister<MainViewModel>();

            SimpleIoc.Default.GetInstance<AccountManager>().Dispose();
            SimpleIoc.Default.Unregister<AccountManager>();
        }
    }
}