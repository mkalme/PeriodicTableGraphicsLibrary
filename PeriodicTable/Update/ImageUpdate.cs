using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;

namespace PeriodicTable
{
    class ImageUpdate
    {
        private static BackgroundWorker backgroundWorker;
        public static Display form;

        public static void setupImageUpdater(Display formDisplay) {
            form = formDisplay;

            backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += new DoWorkEventHandler(backgroundWorker_DoWork);
            backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker_UpdateGraphics);
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.RunWorkerAsync();
        }

        static void backgroundWorker_UpdateGraphics(object sender, ProgressChangedEventArgs e)
        {
            form.pictureBox.Image = Display.graphics.Image;
        }
        static void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            int c = 0;
            while (true)
            {
                Thread.Sleep(10);

                backgroundWorker.ReportProgress(c++);
            }
        }
    }
}
