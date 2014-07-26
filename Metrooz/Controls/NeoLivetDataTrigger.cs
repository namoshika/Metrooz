/*
 * このソースコードは下記のプロジェクトLivetから拝借したものです。
 * Livet Copyright (c) 2010-2011 Livet Project
 * Livet is provided with zlib/libpng license.
 * https://github.com/ugaya40/Livet
 */
using System.Windows;

namespace Metrooz.Controls
{
    /// <summary>
    /// 初期値に対応したDataTriggerです。
    /// </summary>
    public class NeoLivetDataTrigger : Microsoft.Expression.Interactivity.Core.DataTrigger
    {
        protected async override void OnAttached()
        {
            base.OnAttached();

            //OnAttached()時に内容の評価を行わせると仕込んだAction達から例外が
            //生じる事がある。そのため、Task.Yield()で評価を後回しにする。
            if(AssociatedObject is FrameworkElement)
                ((FrameworkElement)AssociatedObject).Loaded += LivetDataTrigger_Loaded;
            else
            {
                await System.Threading.Tasks.Task.Yield();
                EvaluateBindingChange();
            }
        }
        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (AssociatedObject is FrameworkElement)
                ((FrameworkElement)AssociatedObject).Loaded += LivetDataTrigger_Loaded;
        }
        void LivetDataTrigger_Loaded(object sender, RoutedEventArgs e)
        { EvaluateBindingChange(); }
        void EvaluateBindingChange()
        {
            base.EvaluateBindingChange(
                new DependencyPropertyChangedEventArgs(
                    ValueProperty,
                    null,
                    Value));
        }
    }
}
