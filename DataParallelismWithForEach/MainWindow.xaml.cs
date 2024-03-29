﻿using System;
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
using System.Drawing;
using System.Threading;
using System.IO;
using Path = System.IO.Path;

namespace DataParallelismWithForEach
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CancellationTokenSource cancelToken = new CancellationTokenSource();
        public MainWindow()
        {
            InitializeComponent();
        }
        private void cmdCancel_Click(object sender, EventArgs e)
        {
            // This will be updated shortly
            cancelToken.Cancel();
        }
        private void cmdProcess_Click(object sender, EventArgs e)
        {
            // start a new task to process fles.
            Task.Factory.StartNew(() => ProcessFiles());
        }
        private void ProcessFiles()
        {
            // Use ParallelOptions instance to store the CancellationToken.
            ParallelOptions parOpts = new ParallelOptions();
            parOpts.CancellationToken = cancelToken.Token;
            parOpts.MaxDegreeOfParallelism = System.Environment.ProcessorCount;
            // Load up all *.jpg files, and make a new folder for the modified data.
            string[] files = Directory.GetFiles(@".\TestPictures", "*.jpg", SearchOption.
            AllDirectories);
            string newDir = @".\ModifiedPictures";
            Directory.CreateDirectory(newDir);
            try
            {
                // Process the image data in a parallel manner!
                Parallel.ForEach(files, parOpts, currentFile =>
                {
                    parOpts.CancellationToken.ThrowIfCancellationRequested();
                    string filename = Path.GetFileName(currentFile);
                    using (Bitmap bitmap = new Bitmap(currentFile))
                    {
                        bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        bitmap.Save(Path.Combine(newDir, filename));
                        this.Dispatcher.Invoke((Action)delegate
                        {
                            this.Title = $"Processing {filename} on thread {Thread.CurrentThread.ManagedThreadId}";
                        });
                    }
                });
                this.Dispatcher.Invoke((Action)delegate
                {
                    this.Title = "Done!";
                });
            }
            catch (OperationCanceledException ex)
            {
                this.Dispatcher.Invoke((Action)delegate
                {
                    this.Title = ex.Message;
                });
            }
        }

    }
}