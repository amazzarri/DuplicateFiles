using System;
using System.IO;
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
using WinForms = System.Windows.Forms;
using System.Diagnostics;
using System.ComponentModel;

namespace DuplicateFiles
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

        int length = 0;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            dlg = new WinForms.FolderBrowserDialog()
            {
                SelectedPath = @"c:\Fun\Tech"
            };

                        if (dlg.ShowDialog() == WinForms.DialogResult.OK)
            {

            length = 0;
            pbStatus.Value = 0;



            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerAsync();    
                        }
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbStatus.Value = e.ProgressPercentage;
        }

        WinForms.FolderBrowserDialog dlg;

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var todelete = new List<FileInfo>();

            var dir = new DirectoryInfo(dlg.SelectedPath);

            var fileGroups = from c in dir.GetFiles()
                             group c by c.Length into hh
                             where hh.Count() > 1
                             select hh;

            var str = File.CreateText(@"c:\tmp\todelete.txt");

            length = fileGroups.Count();
            int computed = 0;

            foreach (var fg in fileGroups)
            {
                Debug.WriteLine("Key = " + fg.Key);

                foreach (var fl in fg)
                {
                    Debug.WriteLine("   Value = " + fl.Name);

                    foreach (var fl2 in fg)
                    {
                        if (fl != fl2 &&
                            !todelete.Contains(fl) &&
                            FileCompare(fl.FullName, fl2.FullName))
                        {
                            todelete.Add(fl2);
                            str.WriteLine(fl2.FullName);
                            str.Flush();
                        }
                    }
                }

                computed++;

                (sender as BackgroundWorker).ReportProgress(100 * computed / length);
            }

            MessageBox.Show("Parsing completed!");

            todelete.ForEach(x => Debug.WriteLine(x.FullName + " will be deleted"));
        }

        private bool FileCompare(string file1, string file2)
        {
            int file1byte;
            int file2byte;
            FileStream fs1;
            FileStream fs2;

            // Determine if the same file was referenced two times.
            if (file1 == file2)
            {
                // Return true to indicate that the files are the same.
                return true;
            }

            // Open the two files.
            fs1 = new FileStream(file1, FileMode.Open);
            fs2 = new FileStream(file2, FileMode.Open);

            // Check the file sizes. If they are not the same, the files 
            // are not the same.
            if (fs1.Length != fs2.Length)
            {
                // Close the file
                fs1.Close();
                fs2.Close();

                // Return false to indicate files are different
                return false;
            }

            // Read and compare a byte from each file until either a
            // non-matching set of bytes is found or until the end of
            // file1 is reached.
            do
            {
                // Read one byte from each file.
                file1byte = fs1.ReadByte();
                file2byte = fs2.ReadByte();
            }
            while ((file1byte == file2byte) && (file1byte != -1));

            // Close the files.
            fs1.Close();
            fs2.Close();

            // Return the success of the comparison. "file1byte" is 
            // equal to "file2byte" at this point only if the files are 
            // the same.
            return ((file1byte - file2byte) == 0);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            int counter = 0;
            string line;

            // Read the file and display it line by line.
            System.IO.StreamReader file =
               new System.IO.StreamReader(@"c:\tmp\todelete.txt");

            while ((line = file.ReadLine()) != null)
            {
                try
                {
                    File.Delete(line);
                }
                catch(Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }

                counter++;
            }

            file.Close();

            MessageBox.Show(counter.ToString() +
                " files were deleted!");

        }
    }
}
