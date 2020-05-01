using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MarkLogic.Esri.ArcGISPro.AddIn.Controls
{
    public partial class ColorPicker : UserControl
    {
        public static readonly Color DefaultColor = Colors.Black;

        public class Item
        {
            public Item(string name, Color color)
            {
                Name = name;
                Color = color;
                Brush = new SolidColorBrush(color);
            }

            public string Name { get; private set; }

            public Color Color { get; private set; }

            public Brush Brush { get; private set; }

            public override string ToString() => Name;
        }

        private static ReadOnlyCollection<Item> _allColors;

        public ColorPicker()
        {
            InitializeComponent();
            ctlColorPicker.ItemsSource = _allColors ?? (_allColors = GetAllColors());
        }

        private static ReadOnlyCollection<Item> GetAllColors()
        {
            var colors = typeof(Colors).GetProperties(BindingFlags.Public | BindingFlags.Static)
                .Select(prop =>
                {
                    var name = prop.Name.SpaceBetweenWords();
                    var color = (Color)prop.GetValue(null);
                    return new Item(name, color);
                })
                .ToList();
            return new ReadOnlyCollection<Item>(colors);
        }

        public IReadOnlyCollection<Item> AllColors => _allColors;

        public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register("SelectedColor", typeof(Color), typeof(ColorPicker), new FrameworkPropertyMetadata(
                DefaultColor,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, e) => (o as ColorPicker).OnSelectedColorPropertyChanged((Color)e.NewValue)));

        public Color SelectedColor
        {
            get { return (Color)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }

        private void OnSelectedColorPropertyChanged(Color newColor)
        {
            var item = AllColors.FirstOrDefault(i => i.Color == newColor);
            Debug.Assert(item != null);
            ctlColorPicker.SelectedItem = item;
        }

        private void ColorPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedColor = ctlColorPicker.SelectedItem == null ? DefaultColor : (ctlColorPicker.SelectedItem as Item).Color;
        }
    }
}
