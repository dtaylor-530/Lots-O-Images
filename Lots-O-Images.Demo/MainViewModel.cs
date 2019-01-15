using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Lots_O_Images.Demo
{

    public class MainViewModel : ViewModelBase
    {
        private readonly string PATH_TO_IMAGES = @"..\..\icons";

        public ObservableCollection<DynamicImage> DynamicImages { get; }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {

            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
            }
            else
            {

                DynamicImages = new ObservableCollection<DynamicImage>(
                    collection: 
                    Directory.EnumerateFiles(PATH_TO_IMAGES, "*", SearchOption.AllDirectories)
                    .Select(img => new DynamicImage(img)));
            }
        }


    }
}