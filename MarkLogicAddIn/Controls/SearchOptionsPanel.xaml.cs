using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MarkLogic.Esri.ArcGISPro.AddIn.Controls
{
    public partial class SearchOptionsPanel : UserControl
    {
        private bool _clusterDivisionsDragStarted = false;

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
            ApplyOptions?.Execute(null);
        }

        private void ValuesLimit_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(ctlMaxValues.Text) || !ctlValuesLimit.IsChecked.GetValueOrDefault(false))
                InvokeApplyOptions();
        }

        private void MaxValues_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = e.Text.Any(c => !char.IsDigit(c)); // only allow numbers
        }

        private void MaxValues_KeyDown(object sender, KeyEventArgs e)
        {
            var text = ctlMaxValues.Text;
            if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(text))
            {
                try 
                {
                    MaxValues = Convert.ToInt64(text);
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
            InvokeApplyOptions();
        }

        private void UpdateClusterDivisions()
        {
            ClusterDivisions = Convert.ToUInt32(ctlClusterDivisions.Value);
            InvokeApplyOptions();
        }

        private void ClusterDivisions_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_clusterDivisionsDragStarted)
                UpdateClusterDivisions();
        }

        private void ClusterDivisions_DragStarted(object sender, DragStartedEventArgs e)
        {
            _clusterDivisionsDragStarted = true;
        }

        private void ClusterDivisions_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            UpdateClusterDivisions();
            _clusterDivisionsDragStarted = false;
        }
    }
}
