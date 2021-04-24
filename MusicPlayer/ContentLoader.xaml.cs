using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
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
                    Trace.WriteLine("is not web url is local path");
                }
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
    }
}
