using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Mapping;
using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;
using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages;
using System;
using System.Windows.Input;
using System.Windows.Media;

namespace MarkLogic.Esri.ArcGISPro.AddIn.ViewModels
{
    public class SymbologyItemViewModel : ViewModelBase
    {
        public SymbologyItemViewModel(MessageBus messageBus, string valueName)
        {
            MessageBus = messageBus ?? throw new ArgumentNullException("messageBus");
            MessageBus.Subscribe<GetSymbologyMessage>(m =>
            {
                m.Color = Color ?? Colors.Black;
                m.Shape = Shape;
                m.Size = Size;
                m.Opacity = Convert.ToInt32(Opacity);
                m.Resolved = true;
            });
            ValueName = valueName ?? throw new ArgumentNullException("valueName");

            // default TODO: load from config
            Color = Colors.Red;
            Shape = SimpleMarkerStyle.Circle;
            Size = 5;
            Opacity = 60;
        }

        protected MessageBus MessageBus { get; private set; }

        public string ValueName { get; private set; }

        private Color? _color;
        public Color? Color
        {
            get { return _color; }
            set { SetProperty(ref _color, value); }
        }

        private SimpleMarkerStyle _shape;
        public SimpleMarkerStyle Shape
        {
            get { return _shape; }
            set { SetProperty(ref _shape, value); }
        }

        private uint _size;
        public uint Size
        {
            get { return _size; }
            set { SetProperty(ref _size, value); }
        }

        private uint _opacity;
        public uint Opacity
        {
            get { return _opacity; }
            set { SetProperty(ref _opacity, value); }
        }

        private RelayCommand _cmdApply;
        public ICommand Apply => _cmdApply ?? (_cmdApply = new RelayCommand(async () => await MessageBus.Publish(new RedrawMessage(ValueName))));
    }
}
