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
    public class Frame : ObservableObject
    {

        public Frame(string mainImagePath)
        {
            this.MainImage = new DynamicImage(mainImagePath);
        }


        private bool _isInView;
        /// <summary>
        ///     Indicates that the frame is currently in view
        /// </summary>
        public bool IsInView
        {
            get
            {
                return _isInView;
            }
            set
            {
                if (Set(() => IsInView, ref _isInView, value))
                {
                    IsInViewChanged.Raise(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        ///     Fired when the IsVisible value has changed
        /// </summary>
        public event EventHandler IsInViewChanged;

        
        private DynamicImage _mainImage;
        /// <summary>
        ///     
        /// </summary>
        public DynamicImage MainImage
        {
            get
            {
                return _mainImage;
            }
            set
            {
                if (Set(() => MainImage, ref _mainImage, value))
                {
                    MainImageChanged.Raise(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        ///     Fired when the MainImage value has changed
        /// </summary>
        public event EventHandler MainImageChanged;


        public CancellationTokenSource CancelUnload { get; set; }

    }
}
