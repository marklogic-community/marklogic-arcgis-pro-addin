using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MarkLogic.Esri.ArcGISPro.AddIn.Controls
{
    public partial class SearchOptionsPanel : UserControl
    {
        public const long DefaultValuesLimit = 1000;

        public SearchOptionsPanel()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty LimitValuesProperty = DependencyProperty.Register("LimitValues", typeof(bool), typeof(SearchOptionsPanel));
        public bool LimitValues
        {
            get { return (bool)GetValue(LimitValuesProperty); }
            set { SetValue(LimitValuesProperty, value); }
        }

        public static readonly DependencyProperty MaxValuesProperty = DependencyProperty.Register("MaxValues", typeof(long), typeof(SearchOptionsPanel));
        public long MaxValues
        {
            get { return (long)GetValue(MaxValuesProperty); }
            set { SetValue(MaxValuesProperty, value); }
        }

        public static readonly DependencyProperty ClusterResultsProperty = DependencyProperty.Register("ClusterResults", typeof(bool), typeof(SearchOptionsPanel));
        public bool ClusterResults
        {
            get { return (bool)GetValue(ClusterResultsProperty); }
            set { SetValue(ClusterResultsProperty, value); }
        }

        public static readonly DependencyProperty ClusterDivisionsProperty = DependencyProperty.Register("ClusterDivisions", typeof(uint), typeof(SearchOptionsPanel));
        public uint ClusterDivisions
        {
            get { return (uint)GetValue(ClusterDivisionsProperty); }
            set { SetValue(ClusterDivisionsProperty, value); }
        }

        public static readonly DependencyProperty ApplyOptionsProperty = DependencyProperty.Register("ApplyOptions", typeof(ICommand), typeof(SearchOptionsPanel));
        public ICommand ApplyOptions
        {
            get { return (ICommand)GetValue(ApplyOptionsProperty); }
            set { SetValue(ApplyOptionsProperty, value); }
        }

        private void InvokeApplyOptions()
        {
            if (LimitValues && MaxValues <= 0)
                return;
            ApplyOptions?.Execute(null);
        }

        private void ValuesLimit_CheckChanged(object sender, RoutedEventArgs e)
        {
            LimitValues = (sender as CheckBox).IsChecked.GetValueOrDefault(false);
            MaxValues = LimitValues ? DefaultValuesLimit : 0;
            ctlMaxValues.Text = LimitValues ? DefaultValuesLimit.ToString() : "";
            InvokeApplyOptions();
        }

        private void MaxValues_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = e.Text.Any(c => !char.IsDigit(c)); // only allow numbers
        }

        private void MaxValues_KeyDown(object sender, KeyEventArgs e)
        {
            var text = (sender as TextBox).Text;
            if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(text))
            {
                try 
                {
                    MaxValues = Convert.ToInt64(text);
                    e.Handled = true;
                    InvokeApplyOptions();
                }
                catch (Exception)
                {
                    return;
                }
            }
        }

        private void ClusterResults_CheckChanged(object sender, RoutedEventArgs e)
        {
            ClusterResults = (sender as CheckBox).IsChecked.GetValueOrDefault(false);
            InvokeApplyOptions();
        }

        private void ClusterDivisions_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ClusterDivisions = Convert.ToUInt32((sender as Slider).Value);
            InvokeApplyOptions();
        }
    }
}
