using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace MarkLogic.Esri.ArcGISPro.AddIn
{
    public class DropDownButtonBehavior : Behavior<Button>
    {
        private long attachedCount;
        private bool isMenuOpen;

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.AddHandler(Button.ClickEvent, new RoutedEventHandler(AssociatedObject_Click), true);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.RemoveHandler(Button.ClickEvent, new RoutedEventHandler(AssociatedObject_Click));
        }

        private void AssociatedObject_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null && button.ContextMenu != null)
            {
                if (!isMenuOpen)
                {
                    button.ContextMenu.AddHandler(ContextMenu.ClosedEvent, new RoutedEventHandler(ContextMenu_Closed), true);
                    Interlocked.Increment(ref attachedCount);
                    button.ContextMenu.PlacementTarget = button;
                    button.ContextMenu.Placement = PlacementMode.Bottom;
                    button.ContextMenu.IsOpen = true;
                    isMenuOpen = true;
                }
            }
        }

        private void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            isMenuOpen = false;
            var menu = sender as ContextMenu;
            if (menu != null)
            {
                menu.RemoveHandler(ContextMenu.ClosedEvent, new RoutedEventHandler(ContextMenu_Closed));
                Interlocked.Decrement(ref attachedCount);
            }
        }
    }
}
