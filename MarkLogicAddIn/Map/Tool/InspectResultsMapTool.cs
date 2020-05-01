using System.Diagnostics;
using System.Threading.Tasks;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Mapping;
using MarkLogic.Esri.ArcGISPro.AddIn.Controls;
using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;
using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels;
using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages;

namespace MarkLogic.Esri.ArcGISPro.AddIn.Map.Tool
{
    public class InspectResultsMapTool : MapTool
    {
        public InspectResultsMapTool()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Point;
            SketchOutputMode = SketchOutputMode.Map;
        }

        private MessageBus MessageBus => AddInModule.Instance.MessageBus;

        protected override Task OnToolDeactivateAsync(bool hasMapViewChanged)
        {
            return MessageBus.Publish(new SelectMapLocationMessage(null));
        }

        protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            var mapView = MapView.Active;
            Debug.Assert(mapView != null);
            Debug.Assert(geometry is MapPoint);

            var locationMsg = await MessageBus.Publish(new SelectMapLocationMessage((MapPoint)geometry));
            if (!locationMsg.Resolved || locationMsg.Extent == null)
                return false;

            var viewModel = new InspectResultsViewModel(MessageBus, locationMsg.Extent);
            var inspectWnd = new InspectResultsWindow() { DataContext = viewModel };
            inspectWnd.ShowDialog();

            return true;
        }
    }
}
