using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lots_O_Images
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WrapPanel storyboardPanel;
        public MainWindow()
        {
            InitializeComponent();

            /*ItemsPresenter storyboardPresenter = GetVisualChild<ItemsPresenter>(StoryBoardControl);
            storyboardPanel = GetVisualChild<WrapPanel>(storyboardPresenter);*/
        }

        /*private static T GetVisualChild<T>(DependencyObject parent) where T : Visual
        {
            // meh, change me
            T child = default(T);

            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }*/

        private void UpdateVisible()
        {
            if (storyboardPanel == null)
            {
                return;
            }

            foreach (FrameworkElement item in storyboardPanel.Children)
            {
                GeneralTransform childTransform = item.TransformToAncestor(MainScroll);
                Rect rectangle = childTransform.TransformBounds(new Rect(new Point(0, 0),
                    item.RenderSize));

                // Check if the elements Rect intersects with that of the scrollviewer's
                Rect result = Rect.Intersect(new Rect(new Point(0, 0), MainScroll.RenderSize),
                    rectangle);

                // if result is Empty then the element is not in view
                ((Frame)item.DataContext).IsInView = (result != Rect.Empty);
                // ((Frame)item.DataContext).IsInView = true;
            }
        }


        private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateVisible();
        }

        double accruedChange = 0;

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // reset on direction changed
            if ((accruedChange > 0 && e.VerticalChange < 0) || (accruedChange < 0 && e.VerticalChange > 0))
            {
                accruedChange = 0;
            }
            accruedChange += e.VerticalChange;
            if (Math.Abs(accruedChange) > 100)
            {
                UpdateVisible();
            }
        }

        private void WrapPanel_Loaded(object sender, RoutedEventArgs e)
        {
            storyboardPanel = (WrapPanel)sender;
            UpdateVisible();
        }
    }
}
