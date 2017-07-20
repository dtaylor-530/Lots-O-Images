using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Lots_O_Images
{
    public class DynamicImage : ObservableObject
    {
        SemaphoreSlim loadingSemaphore = new SemaphoreSlim(1, 1);


        public DynamicImage(string pathToSource)
        {
            this.PathToSource = pathToSource;
        }

        private string _pathToSource;
        /// <summary>
        ///     
        /// </summary>
        public string PathToSource
        {
            get
            {
                return _pathToSource;
            }
            private set
            {
                Set(() => PathToSource, ref _pathToSource, value);
            }
        }

        private BitmapImage _bitmap;
        /// <summary>
        ///     
        /// </summary>
        public BitmapImage Bitmap
        {
            get
            {
                return _bitmap;
            }
            private set
            {
                if (Set(() => Bitmap, ref _bitmap, value))
                {
                    BitmapChanged.Raise(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        ///     Fired when the FullImage value has changed
        /// </summary>
        public event EventHandler BitmapChanged;



        private bool _isUnloading;
        /// <summary>
        ///     
        /// </summary>
        public bool IsUnloading
        {
            get
            {
                return _isLoading;
            }
            private set
            {
                if (Set(() => IsUnloading, ref _isUnloading, value))
                {
                    IsUnloadingChanged.Raise(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        ///     Fired when the IsLoading value has changed
        /// </summary>
        public event EventHandler IsUnloadingChanged;


        private bool _isLoading;
        /// <summary>
        ///     
        /// </summary>
        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            private set
            {
                if (Set(() => IsLoading, ref _isLoading, value))
                {
                    IsLoadingChanged.Raise(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        ///     Fired when the IsLoading value has changed
        /// </summary>
        public event EventHandler IsLoadingChanged;


        private bool _isLoaded;
        /// <summary>
        ///     
        /// </summary>
        public bool IsLoaded
        {
            get
            {
                return _isLoaded;
            }
            private set
            {
                if (Set(() => IsLoaded, ref _isLoaded, value))
                {
                    IsLoadedChanged.Raise(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        ///     Fired when the IsLoading value has changed
        /// </summary>
        public event EventHandler IsLoadedChanged;



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
                src.UriSource = new Uri(this.PathToSource);
                src.CacheOption = BitmapCacheOption.OnLoad;
                src.EndInit();
                src.Freeze(); // freeze the resource so we can pass it to the WPF render thread, or something

                App.Current.Dispatcher.Invoke(new Action(() => this.Bitmap = src));
            });

            IsLoaded = true;
            IsLoading = false;
            loadingSemaphore.Release();
           
        }

        public async Task Unload(CancellationToken cancel)
        {
            this.IsUnloading = true;
            // we cant cancel loading, so lets just wait for it to finish then unload right after
            await loadingSemaphore.WaitAsync();

            if (cancel.IsCancellationRequested)
            {
                this.IsUnloading = false;
                loadingSemaphore.Release();
                return;
            }

            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                this.Bitmap = null;
                // todo next: image scaling. saving scaled images (large thumbails, check crcs against DB, etc)
                //GC.Collect(); // helps alot, but shiiiiiiet i dont liek calling it. also what if we want to cache the images etc
            }));

            this.IsLoaded = false;
            this.IsUnloading = false;
            loadingSemaphore.Release();
        }
    }
}
