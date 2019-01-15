
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Lots_O_Images
{
    public class DynamicImage : INotifyPropertyChanged
    {
        SemaphoreSlim loadingSemaphore = new SemaphoreSlim(1, 1);
        string pathToSource;

        public DynamicImage(string pathToSource)
        {
            this.pathToSource = pathToSource;
        }

   

        private BitmapImage _bitmap;
        /// <summary>
        ///     
        /// </summary>
        public BitmapImage Bitmap
        {
            //get;set;
            get
            {
                return _bitmap;
            }
            private set
            {
                _bitmap = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Bitmap)));
            }
        }
        /// <summary>
        ///     Fired when the FullImage value has changed
        /// </summary>
        public event EventHandler BitmapChanged;


        /// <summary>
        ///     
        /// </summary>
        public bool IsUnloading
        {
            get; set;
        }


        /// <summary>
        ///     
        /// </summary>
        public bool IsLoading
        {
            get; set;
        }



        // private bool _isLoaded;
        /// <summary>
        ///     
        /// </summary>
        public bool IsLoaded { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        public async Task Load()
        {
            if (IsLoading)
            {
                throw new InvalidOperationException("Can not start a new Load task while another is in progress.");
            }

            await loadingSemaphore.WaitAsync();
            IsLoading = true;
            await Task.Run(() =>
            {
                BitmapImage src = new BitmapImage();
                src.BeginInit();
                src.UriSource = new Uri(this.pathToSource, UriKind.Relative);
                src.CacheOption = BitmapCacheOption.OnLoad;
                src.EndInit();
                src.Freeze(); // freeze the resource so we can pass it to the WPF render thread, or something
                return src;
                //Current.Dispatcher.Invoke();
            }).ContinueWith(async (a) => { this.Bitmap = await a; ; }, TaskScheduler.FromCurrentSynchronizationContext());

            IsLoaded = true;
            IsLoading = false;
            loadingSemaphore.Release();

        }

        public async Task Unload(CancellationToken cancel)
        {
            this.IsUnloading = true;
            // we cant cancel loading, so lets just wait for it to finish then unload right after
            await loadingSemaphore.WaitAsync().ContinueWith((a) =>
            {
                if (cancel.IsCancellationRequested)
                {
                    this.IsUnloading = false;
                    loadingSemaphore.Release();
                    return;
                }
                this.Bitmap = null;

            });



            //App.Current.Dispatcher.Invoke(new Action(() =>
            //{
            //    this.Bitmap = null;
            //    // todo next: image scaling. saving scaled images (large thumbails, check crcs against DB, etc)
            //    //GC.Collect(); // helps alot, but shiiiiiiet i dont liek calling it. also what if we want to cache the images etc
            //}));

            this.IsLoaded = false;
            this.IsUnloading = false;
            loadingSemaphore.Release();
        }

        public CancellationTokenSource CancelUnload { get; set; }
    }
}
