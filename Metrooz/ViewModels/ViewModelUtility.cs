using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrooz.ViewModels
{
    public class ViewModelUtility
    {
        public readonly static bool IsDesginMode = System.ComponentModel
            .DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject());
    }
}
