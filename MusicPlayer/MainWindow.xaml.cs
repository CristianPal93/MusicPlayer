using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Vlc.DotNet.Wpf;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System.Reflection;
using YoutubeExplode;
using NYoutubeDL;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace MusicPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {

        private bool isPlaying = false;
        private ContentLoader cL;
        public static bool isOpen = false;
        public static string videoId = "";
        List<Video> videos = new List<Video>();
        private YoutubeDL youtubeDl = new YoutubeDL();
        private DirectoryInfo vlcLibDirectory;
        private VlcControl control;
        public int songIndex = 0;
        private Button button;
        private PackIcon pack;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                initVlcPlayer();
                initYoutubeDll();
             

            }
            catch (System.Windows.Markup.XamlParseException e)
            {

                Console.WriteLine(e);
            }
        }



        public void YoutubeBuffer(string url)
        {
                
        
                try
                {
                    youtubeDl.DownloadAsync(url);
                    Trace.WriteLine(String.Format("Buffering video {0}", url));

                }
                catch (Exception ex)
                {
                    Trace.WriteLine(String.Format("Error at {0}", ex));
                }

       

            
        }


        private void initYoutubeDll() {
            youtubeDl.Options.FilesystemOptions.Output = Directory.GetCurrentDirectory() + @"\tmp\temp.m4a";
            youtubeDl.Options.PostProcessingOptions.AudioFormat = NYoutubeDL.Helpers.Enums.AudioFormat.m4a;
            youtubeDl.YoutubeDlPath = Directory.GetCurrentDirectory() + @"\libytdl\youtube-dl.exe";
            youtubeDl.Options.PostProcessingOptions.ExtractAudio = true;
            youtubeDl.Info.PropertyChanged += Info_PropertyChanged;

        }

        private void initVlcPlayer() {

            var currentAssembly = Assembly.GetEntryAssembly();
            var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
            // Default installation path of VideoLAN.LibVLC.Windows
            vlcLibDirectory = new DirectoryInfo(Path.Combine(currentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));
            this.control?.Dispose();
            this.control = new VlcControl();
            //  this.ControlContainer.Content = this.control;
            this.vlcPlayer.Content = this.control;

            this.control.SourceProvider.CreatePlayer(this.vlcLibDirectory);
            // This can also be called before EndInit
            this.control.SourceProvider.MediaPlayer.Log += (_, args) =>
            {
                string message = $"libVlc : {args.Level} {args.Message} @ {args.Module}";
                System.Diagnostics.Debug.WriteLine(message);
            };

        }

        private void Info_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Trace.WriteLine(String.Format("Calling Info Prop {0}", e));
        }

        public async Task SyncList()
        {
            Trace.WriteLine("intra!");

            if (!String.IsNullOrEmpty(videoId))
            {
                await Task.Run(async () => await GetYoutubePlayList(videoId));

            }
            listView.ItemsSource = videos;
        }

        private void CloseApp(object sender, RoutedEventArgs e)
        {
            deleteAllTempFiles(Directory.GetCurrentDirectory() + @"\tmp\");
            Environment.Exit(0);

        }

        private void WindowMinimized(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;

        }

        private void WindowMaximized(object sender, RoutedEventArgs e)
        {
            button = sender as Button;
            pack = (button.FindName("WindowSettings") as PackIcon);
            if (pack.Kind.Equals(PackIconKind.WindowMaximize))
            {
                this.WindowState = WindowState.Maximized;
                (button.FindName("WindowSettings") as PackIcon).Kind = PackIconKind.DockWindow;


            }
            else
            {
                this.WindowState = WindowState.Normal;
                (button.FindName("WindowSettings") as PackIcon).Kind = PackIconKind.WindowMaximize;


            }
            Console.WriteLine(pack.Kind);

        }

        private void Anterior_Click(object sender, RoutedEventArgs e)
        {

        }



        private void Urmator_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {
            button = sender as Button;
            pack = (button.FindName("PlayPauseIcon") as PackIcon);
            if (pack.Kind.Equals(PackIconKind.Play))
            {
                this.isPlaying = true;
                (button.FindName("PlayPauseIcon") as PackIcon).Kind = PackIconKind.Stop;


                control.SourceProvider.MediaPlayer.Play(new Uri(Directory.GetCurrentDirectory() + @"\tmp\temp.m4a"));


                this.vlcPlayer.Visibility = Visibility.Hidden;





            }
            else
            {


                if (control.SourceProvider.MediaPlayer.IsPlaying())
                {
                    (button.FindName("PlayPauseIcon") as PackIcon).Kind = PackIconKind.Play;
                    control.SourceProvider.MediaPlayer.SetPause(true);
                    this.isPlaying = false;
                }
            }


        }


        private void Browse(object sender, RoutedEventArgs e)
        {


            if (isOpen)
            {
                cL.Close();
                isOpen = false;
            }
            cL = new ContentLoader();
            videoId = "";
            cL.Show();
            cL.Owner = this;
            isOpen = true;



        }

        private void Window_move(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            DragMove();
        }
        public async Task GetYoutubePlayList(string playlistId)
        {

            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "AIzaSyAV2lEVzNa6-vjC6rjLU4j5PklKGHjYepA",
                ApplicationName = this.GetType().ToString(),
            });

            videos = new List<Video>();
            var uploadsListId = playlistId;

            var nextPageToken = "";
            int trackNo = 0;

            while (nextPageToken != null)
            {
                var playlistItemsListRequest = youtubeService.PlaylistItems.List("snippet");
                playlistItemsListRequest.PlaylistId = uploadsListId;
                playlistItemsListRequest.MaxResults = 50;
                playlistItemsListRequest.PageToken = nextPageToken;

                // Retrieve the list of videos uploaded to the authenticated user's channel.
                var playlistItemsListResponse = await playlistItemsListRequest.ExecuteAsync();
                foreach (var playlistItem in playlistItemsListResponse.Items)
                {
                    if (playlistItem.Snippet.Title.ToString() != "Deleted video" || playlistItem.Snippet.Title.ToString() != "Private video")
                    {
                        // Print information about each video.
                        String thumbnail = "https://img.youtube.com/vi/" + playlistItem.Snippet.ResourceId.VideoId.ToString() + "/default.jpg";
                        String Mainthumbnail = "https://img.youtube.com/vi/" + playlistItem.Snippet.ResourceId.VideoId.ToString() + "/hqdefault.jpg";
                        String Artist = "";
                        String Title = "";
                        try
                        {
                            String[] Artist_Title = playlistItem.Snippet.Title.ToString().Split('-');
                            Artist = Artist_Title[0];
                            Title = Artist_Title[1];
                        } catch (Exception ex)
                        {
                            Trace.WriteLine(String.Format("Can't split Artist title"));
                        }
                        videos.Add(new Video() { TrackNo = trackNo, Name = playlistItem.Snippet.Title.ToString(), VideoID = playlistItem.Snippet.ResourceId.VideoId, Thumbnail = thumbnail, ThumbnailFront = Mainthumbnail, Artist = Artist, Title = Title });
                        trackNo += 1;


                    }
                }

                nextPageToken = playlistItemsListResponse.NextPageToken;
            }



            Trace.WriteLine(String.Format("Videos:\n{0}\n", string.Join("\n", videos[0].Name)));



        }

        private async Task RenderSong()
        {
            PlayPauseIcon.Kind = PackIconKind.Play;

            await Task.Delay(6000);
            if (control.SourceProvider.MediaPlayer.IsPlaying())
            {
                control.SourceProvider.MediaPlayer.Stop();
                control.SourceProvider.MediaPlayer.Play(new Uri(Directory.GetCurrentDirectory() + @"\tmp\temp.m4a"));
            }
            control.SourceProvider.MediaPlayer.Play(new Uri(Directory.GetCurrentDirectory() + @"\tmp\temp.m4a"));
            PlayPauseIcon.Kind = PackIconKind.Stop;
            this.isPlaying = true;
        }

            private void listView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            songIndex = this.listView.Items.IndexOf(listView.SelectedItem);
            if (songIndex != System.Windows.Forms.ListBox.NoMatches)
            {
                Trace.WriteLine(String.Format("Selected item is {0}", videos[songIndex].ThumbnailFront));


            }
            Thumbnail.ImageSource = new BitmapImage(new Uri(videos[songIndex].ThumbnailFront.ToString()));
            Artist.Text = videos[songIndex].Artist;
            Title.Text = videos[songIndex].Title;
            string songUrl = "https://www.youtube.com/watch?v=" + videos[songIndex].VideoID;
            deleteAllTempFiles(Directory.GetCurrentDirectory() + @"\tmp\");
            YoutubeBuffer(songUrl);
            RenderSong();
            Trace.WriteLine(String.Format("Done rendering!"));

        }


        private void deleteAllTempFiles(String url) {

            DirectoryInfo di = new DirectoryInfo(url);
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
        }
     
    }
}








