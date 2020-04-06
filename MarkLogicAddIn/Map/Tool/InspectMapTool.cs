using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using MarkLogic.Client.Search;
using MarkLogic.Client.Search.Query;
using MarkLogic.Esri.ArcGISPro.AddIn.Commands;
using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;
using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages;

namespace MarkLogic.Esri.ArcGISPro.AddIn.Map.Tool
{
    public class InspectMapTool : MapTool
    {
        public InspectMapTool()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Point;
            SketchOutputMode = SketchOutputMode.Map;
        }

        private MessageBus MessageBus => AddInModule.Instance.MessageBus;

        private ProgressDialog ProgressDlg { get; set; }

        private GeospatialBox Extent { get; set; }

        protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            var mapView = MapView.Active;
            Debug.Assert(mapView != null);
            Debug.Assert(geometry is MapPoint);

            var msg = await MessageBus.Publish(new SelectMapLocationMessage((MapPoint)geometry));
            if (!msg.Resolved || msg.Extent == null)
                return false;
            Extent = msg.Extent;

            ProgressDlg = new ProgressDialog($"Retrieving items...");
            try
            {
                ProgressDlg.Show();
                var cmd = new SearchCommand(MessageBus, ReturnOptions.Results | ReturnOptions.Facets, false, BuildQuery, ProcessResults);
                cmd.Execute(null);

                return true;
            }
            catch (Exception e)
            {
                e.HandleAsUserNotification();
                return false;
            }
            finally
            {
                ProgressDlg.Hide();
                ProgressDlg.Dispose();
            }
        }

        private Task BuildQuery(SearchQuery query)
        {
            query.Viewport = Extent;
            return Task.CompletedTask;
        }

        private Task ProcessResults(SearchResults results)
        {
            //MessageBox.Show($"Facets: {string.Join("\r\n", results.Facets.Values.SelectMany(v => v.Values).Select(fv => fv.FacetName + ":" + fv.ValueName))},\r\nResults: {string.Join("\r\n", results.DocumentResults.Select(d => d.Uri))}");
            MessageBox.Show($"Result Total: {results.Total},\r\nResults: {string.Join("\r\n", results.DocumentResults.Select(d => d.Uri))}");
            return Task.CompletedTask;
        }
    }
}
