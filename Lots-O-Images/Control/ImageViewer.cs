using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Lots_O_Images
{
    public class ImageViewer:ItemsControl
    {
        double accruedChange = 0;
        private ScrollViewer mainScroll;
        private SemaphoreSlim LoadRestrictor;

        public ImageViewer()
        {        
            // testing showed that once the number of DynamicImages loading exceeded the processor count, performance started dropping
            int x = Environment.ProcessorCount;
            LoadRestrictor = new SemaphoreSlim(1, x);
            this.Loaded += (a, b) => UpdateVisible();
        }



        static ImageViewer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageViewer), new FrameworkPropertyMetadata(typeof(ImageViewer)));
        }

        public override void OnApplyTemplate()
        {
            mainScroll = this.GetTemplateChild("MainScroll") as ScrollViewer;
            mainScroll.ScrollChanged += ScrollViewer_ScrollChanged;
            mainScroll.SizeChanged += (a, b) => UpdateVisible();
           
        }


        
        private void UpdateVisible()
        {

            for(int i=0;i<Items.Count;i++)
            {
                UIElement item = (UIElement)this.ItemContainerGenerator.ContainerFromIndex(i);

               bool b= IsInView(
                    childTransform: item.TransformToAncestor(mainScroll),
                    renderSize: item.RenderSize);

                Temp_IsInViewChanged(b, ((DynamicImage)this.Items[i]));
            }
        }


        private async void Temp_IsInViewChanged(bool b,DynamicImage sender)
        {
            if (b)
            {
                await LoadDynamicImage((DynamicImage)sender);
            }
            else
            {
                await UnloadDynamicImage((DynamicImage)sender);
            }
        }


        private async Task LoadDynamicImage(DynamicImage DynamicImage)
        {
            if (DynamicImage.IsLoading && DynamicImage.IsUnloading)
            {
                if (DynamicImage.CancelUnload != null)
                {
                    DynamicImage.CancelUnload.Cancel();
                }
            }
            else
            {
                await LoadRestrictor.WaitAsync(); // wait for our turn
                await DynamicImage.Load();
                LoadRestrictor.Release();
            }
        }

        private async Task UnloadDynamicImage(DynamicImage DynamicImage)
        {
            DynamicImage.CancelUnload = new CancellationTokenSource();
            await DynamicImage.Unload(DynamicImage.CancelUnload.Token);
        }


        private bool IsInView(GeneralTransform childTransform,Size renderSize)
        {
            Rect rectangle = childTransform.TransformBounds(new Rect(new Point(0, 0), renderSize));

            // Check if the elements Rect intersects with that of the scrollviewer's
            Rect result = Rect.Intersect(new Rect(new Point(0, 0), mainScroll.RenderSize), rectangle);

            // if result is Empty then the element is not in view
            return (result != Rect.Empty);
        }





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



     
    }
}
