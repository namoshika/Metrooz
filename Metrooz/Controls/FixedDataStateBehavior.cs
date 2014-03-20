using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Microsoft.Expression.Interactivity.Core;

namespace Metrooz.Controls
{
    public class FixedDataStateBehavior : DataStateBehavior
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += (sender, routedEventArgs) =>
            {
                var bindingExpression = BindingOperations.GetBindingExpression(this, BindingProperty);
                SetCurrentValue(BindingProperty, new object());
                bindingExpression.UpdateTarget();
            };
        }
    }
}
