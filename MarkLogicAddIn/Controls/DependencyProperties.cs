using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace MarkLogic.Esri.ArcGISPro.AddIn.Controls
{
    public static class DependencyProperties
    {
        public static readonly DependencyProperty InlinesProperty = DependencyProperty.RegisterAttached(
            "Inlines",
            typeof(IEnumerable<Inline>),
            typeof(DependencyProperties),
            new FrameworkPropertyMetadata(OnInlinesPropertyChanged));

        private static void OnInlinesPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var textBlock = obj as TextBlock;
            if (textBlock == null)
                return;
            textBlock.Inlines.Clear();
            textBlock.Inlines.AddRange((IEnumerable<Inline>)e.NewValue);
        }

        public static IEnumerable<Inline> GetInlines(DependencyObject obj)
        {
            return (IEnumerable<Inline>)obj.GetValue(InlinesProperty);
        }

        public static void SetInlines(DependencyObject obj, IEnumerable<Inline> value)
        {
            obj.SetValue(InlinesProperty, value);
        }
    }
}
