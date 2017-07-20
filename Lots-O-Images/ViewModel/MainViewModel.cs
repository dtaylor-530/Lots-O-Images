using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Lots_O_Images.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly string PATH_TO_IMAGES = @"D:\pictures\cars";

        private SemaphoreSlim LoadRestrictor;

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
                // testing showed that once the number of frames loading exceeded the processor count, performance started dropping
                int x = Environment.ProcessorCount;
                LoadRestrictor = new SemaphoreSlim(x, x);
                
                foreach (string img in Directory.EnumerateFiles(PATH_TO_IMAGES, "*", SearchOption.AllDirectories))
                {
                    Frame temp = new Frame(img);
                    temp.IsInViewChanged += Temp_IsInViewChanged;
                    Frames.Add(temp);
                }
            }
        }

        private async void Temp_IsInViewChanged(object sender, EventArgs e)
        {
            if (((Frame)sender).IsInView)
            {
                await LoadFrame((Frame)sender);
            }
            else
            {
                await UnloadFrame((Frame)sender);
            }
        }


        /*
        /// <summary>
        /// Can use this to compare performance & memory usage
        /// </summary>
        private async void LoadAll()
        {
            List<Task> loadingTasks = new List<Task>();
            foreach (Frame frame in Frames)
            {
                loadingTasks.Add(LoadFrame(frame));
            }
            await Task.WhenAll(loadingTasks);
        }*/

        


        private async Task LoadFrame(Frame frame)
        {
            if (frame.MainImage.IsLoading && frame.MainImage.IsUnloading)
            {
                if (frame.CancelUnload != null)
                {
                    frame.CancelUnload.Cancel();
                }
            }
            else
            {
                await LoadRestrictor.WaitAsync(); // wait for our turn
                await frame.MainImage.Load();
                LoadRestrictor.Release();
            }
        }

        private async Task UnloadFrame(Frame frame)
        {
            frame.CancelUnload = new CancellationTokenSource();
            await frame.MainImage.Unload(frame.CancelUnload.Token);
        }


        private ObservableCollection<Frame> _frames = new ObservableCollection<Frame>();
        /// <summary>
        ///     
        /// </summary>
        public ObservableCollection<Frame> Frames
        {
            get
            {
                return _frames;
            }
            set
            {
                if (Set(() => Frames, ref _frames, value))
                {
                    FramesChanged.Raise(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        ///     Fired when the Frames value has changed
        /// </summary>
        public event EventHandler FramesChanged;
        
    }
}