using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace MarkLogic.Esri.ArcGISPro.AddIn.Controls
{
    public partial class PointSymbologyPanel : UserControl
    {
        public class ShapeItem
        {
            public ShapeItem(SimpleMarkerStyle markerStyle)
            {
                Name = Enum.GetName(typeof(SimpleMarkerStyle), markerStyle).SpaceBetweenWords();
                MarkerStyle = markerStyle;
            }

            public string Name { get; private set; }

            public SimpleMarkerStyle MarkerStyle { get; private set; }
        }

        private static ReadOnlyCollection<ShapeItem> _allShapes;

        public PointSymbologyPanel()
        {
            InitializeComponent();
            if (_allShapes == null)
                _allShapes = GetAllShapes();
            ctlShape.ItemsSource = _allShapes;
        }

        private static ReadOnlyCollection<ShapeItem> GetAllShapes()
        {
            var list = new List<ShapeItem>();
            list.AddRange(Enum.GetValues(typeof(SimpleMarkerStyle)).Cast<SimpleMarkerStyle>().Select(m => new ShapeItem(m)));
            return new ReadOnlyCollection<ShapeItem>(list);
        }
    }
}
