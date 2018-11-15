using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace SkiaSharpBackgroundThreading
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        //private void SKElement_OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        //{            
        //    using (var bitmap = new SKBitmap(e.Info.Width, e.Info.Height))
        //    {
        //        using (var canvas = new SKCanvas(bitmap))                
        //        using (var paint = new SKPaint())
        //        {
        //            canvas.DrawCircle(e.Info.Width / 2f, e.Info.Height / 2f, 100, paint);
        //        }
        //        e.Surface.Canvas.DrawBitmap(bitmap, 0, 0);
        //    }
        //}

        private Task<SKBitmap> backgroundTask;        
        private SKBitmap backgroundBitmap;

        private async void SKElement_OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            Debug.WriteLine($"Enter OnPaintSurface: ManagedThreadId={Thread.CurrentThread.ManagedThreadId}");
            
            if (backgroundTask == null)
            {
                backgroundTask = Task.Run(() =>
                {
                    Debug.WriteLine($"Enter Background: ManagedThreadId={Thread.CurrentThread.ManagedThreadId}");

                    using (var bitmap = new SKBitmap(e.Info.Width, e.Info.Height))
                    {
                        using (var canvas = new SKCanvas(bitmap))
                        using (var paint = new SKPaint())
                        {
                            canvas.DrawCircle(e.Info.Width / 2f, e.Info.Height / 2f, 100, paint);
                        }

                        return bitmap.Copy();
                    }
                });
            }
            else
            {
                // When task is awaited below we will get a second call to this method but we just ignore that for now
                Debug.WriteLine($"Exit OnPaintSurface Because Background Processing: ManagedThreadId={Thread.CurrentThread.ManagedThreadId}");
                return;
            }

            if (backgroundTask != null)
            {
                Debug.WriteLine($"Before Background Await: ManagedThreadId={Thread.CurrentThread.ManagedThreadId}");
                backgroundBitmap = await backgroundTask;
                Debug.WriteLine($"After Background Await: ManagedThreadId={Thread.CurrentThread.ManagedThreadId}");
            }

            if (backgroundBitmap != null)
            {                
                Debug.WriteLine($"DrawBitmap: ReadyToDraw={backgroundBitmap.ReadyToDraw}");
                Debug.WriteLine($"DrawBitmap: ManagedThreadId={Thread.CurrentThread.ManagedThreadId}");

                // This call will throw an AccessViolationException, but why?
                e.Surface.Canvas.DrawBitmap(backgroundBitmap, 0, 0);
            }
        }
    }
}
