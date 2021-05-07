using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MusicPlayer
{
    /// <summary>
    /// Interaction logic for ContentLoader.xaml
    /// </summary>
    public partial class ContentLoader : Window
    {
        private string OneFile = "";
        public ContentLoader()
        {
            InitializeComponent();
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            this.Close();
            MainWindow.isOpen = false;
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            var url = TextBoxInput.Text.ToString();
            string videoId = "";
            if (!String.IsNullOrEmpty(url))
            {
                bool web_url = isWebUrl(url);

                if (web_url)
                {

                    videoId = getListID(url);
                    if (!String.IsNullOrEmpty(videoId))
                    {

                        MainWindow.videoId = videoId;
                        System.Threading.Tasks.Task task = ((MainWindow)this.Owner).SyncList();
                        this.Close();

                    }
                }
                else
                {
                    if (IsValidPath(url)){

                        
                       ((MainWindow)this.Owner).SyncLocalList(url);
                        this.Close();
                    }
                    else {
                        Trace.WriteLine("is not a valid path!");
                    }
                }
            }
            else if (!String.IsNullOrEmpty(OneFile))
            {
                ((MainWindow)this.Owner).SyncLocalOneFile(OneFile);
                this.Close();
            }
            else
            {

                Trace.WriteLine("Text box is empty!! throw error msg");
            }
           
        }

        private string getListID(string url)
        {
           
            const string start_id = "list=";
            const string end_idx = "&";
            
            int indx_start = url.IndexOf(start_id);
            int indx_end = url.IndexOf(end_idx, indx_start + (start_id.Length));
            int end_idx_id = indx_end - (indx_start + (start_id.Length));
            Trace.WriteLine(String.Format("start idx is: {0} , indx_end is {1} , end_idx id is {2}", indx_start,indx_end,end_idx_id));
            string id = url.Substring(indx_start+(start_id.Length),end_idx_id);
            Trace.WriteLine(String.Format("intra! de la list control! {0}", id));
            if (!String.IsNullOrEmpty(id))
            {
                return id;
            }
           

                return null;
            
        }


        private bool isWebUrl(string url)
        {
            if (url.Contains("https"))
            {
                return true;
            }
            return false;
        }

        private bool IsValidPath(string path)
        {
            Regex driveCheck = new Regex(@"^[a-zA-Z]:\\$");
            if (!driveCheck.IsMatch(path.Substring(0, 3))) return false;
            string strTheseAreInvalidFileNameChars = new string(System.IO.Path.GetInvalidPathChars());
            strTheseAreInvalidFileNameChars += @":/?*" + "\"";
            Regex containsABadCharacter = new Regex("[" + Regex.Escape(strTheseAreInvalidFileNameChars) + "]");
            if (containsABadCharacter.IsMatch(path.Substring(3, path.Length - 3)))
                return false;

            DirectoryInfo dir = new DirectoryInfo(System.IO.Path.GetFullPath(path));
            if (!dir.Exists)
                dir.Create();
            return true;
        }

        private void File_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            
            dlg.DefaultExt = ".mp3";
            dlg.Filter = "Mp3 Files (*.mp3)|*.mp3";


            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                // Open document 
               OneFile = dlg.FileName;
                
               

            }
            
        }
    }
}
