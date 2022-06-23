using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Anaconda
{
    public partial class MainWindow : Window
    {
        public string PathEXE;
        public string PathFolder;
        public string OutputName;

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        void Start(object sender, EventArgs e)
        {

        }

        private void windowClosed(object sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        void doSelectEXE(object sender, RoutedEventArgs e)
        {
            DecomBtn.Content = "Decompile";
            Microsoft.Win32.OpenFileDialog openFileDialog1 = new Microsoft.Win32.OpenFileDialog();
            openFileDialog1.Filter = "Executables (*.exe)|*.exe";

            bool? response = openFileDialog1.ShowDialog();

            if (response == true)
            {
                PathEXE = openFileDialog1.FileName;
                Icon ico = IconTools.GetIconForFile(PathEXE, ShellIconSize.LargeIcon);
                System.Drawing.Bitmap icoBm = ico.ToBitmap();

                EXEImg.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(icoBm.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                icoBm.Dispose();

                EXEName.Content = Path.GetFileNameWithoutExtension(openFileDialog1.SafeFileName);
                EXEPath.Text = PathEXE;
            }
        }

        void doSelectFolder(object sender, RoutedEventArgs e)
        {
            DecomBtn.Content = "Decompile";
            FolderBrowserDialog FolderBrowser1 = new FolderBrowserDialog();

            DialogResult result = FolderBrowser1.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                PathFolder = FolderBrowser1.SelectedPath;

                FolderName.Content = Path.GetFileName(PathFolder);
                FolderPath.Text = PathFolder;
            }
        }

        void DecompileStart(object sender, RoutedEventArgs e)
        {
            Thread thread1 = new Thread(Decompile);
            thread1.Name = "Thread 1";
            thread1.Start();
        }

        void Decompile()
        {
            try
            {
                using (Process myProcess = new Process())
                {
                    Dispatcher.Invoke(() =>
                    {
                        DecomBtn.Content = "Decompiling...";
                        DecomBtn.IsEnabled = false;
                    });
                    myProcess.StartInfo.UseShellExecute = false;
                    myProcess.StartInfo.FileName = "gui.bat";
                    myProcess.StartInfo.CreateNoWindow = true;
                    myProcess.StartInfo.RedirectStandardOutput = false;
                    myProcess.StartInfo.Arguments = @"""" + PathEXE + @""" """ + PathFolder + @""" " + OutputName;
                    myProcess.Start();

                    while (!myProcess.HasExited)
                    {
                        if (myProcess.HasExited == true)
                            break;
                    }

                    Dispatcher.Invoke(() =>
                    {
                        DecomBtn.Content = "Done!";
                        DecomBtn.IsEnabled = true;
                    });
                }
            }
            catch (Exception ee)
            {
                DateTime now = DateTime.Now;
                string asString = now.ToString("MM-dd-yy HH-mm-ss");

                System.IO.Directory.CreateDirectory("Logs");
                using (StreamWriter outputFile = new StreamWriter($"Logs/Log-{asString}.txt"))
                {
                    outputFile.WriteLine(ee.Message);
                }
                Dispatcher.Invoke(() =>
                {
                    DecomBtn.Content = "Decompile";
                    errorlog.Content = $"Error! Log saved to \"Logs/Log-{asString}.txt\"";
                });
            }
        }

        private void doChangeOutputName(object sender, TextChangedEventArgs e)
        {
            DecomBtn.Content = "Decompile";
            OutputName = OutputNameBox.Text;
            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());

            foreach (char c in invalid)
            {
                OutputName = OutputName.Replace(c.ToString(), "");
            }

            OutputName = @"""" + OutputName + @"""";
        }
    }
}
