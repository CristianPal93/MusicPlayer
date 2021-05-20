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
using NYoutubeDL;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Timers;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;

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
        private int trackNo = 0;

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
            if (!String.IsNullOrEmpty(videoId))
            {
                await Task.Run(async () => await GetYoutubePlayList(videoId));

            }
            Trace.WriteLine(String.Format("Listview has {0} items", listView.Items.Count));

            listView.ItemsSource = null;
            listView.ItemsSource = videos;
        }

        public void SyncLocalList(String path)
        {

            string[] allfiles = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            foreach (var files in allfiles)
            {
                if (files.EndsWith(".mp3"))
                {
                    String fileName = Path.GetFileName(files);
                    videos.Add(new Video() { TrackNo = trackNo, Name = fileName, VideoID = files, Thumbnail = "Resources/photo1.jpg", ThumbnailFront = "Resources /photo1.jpg", Artist = "", Title = fileName, isLocal = true });
                    trackNo += 1;


                }
            }
            listView.ItemsSource = null;
            listView.ItemsSource = videos;
            Trace.WriteLine(String.Format("Listview has {0} items",listView.Items.Count));
        }
        public void SyncLocalOneFile(String path)
        {
            String fileName = Path.GetFileName(path);
            videos.Add(new Video() { TrackNo = trackNo, Name = fileName, VideoID = path, Thumbnail = "Resources/photo1.jpg", ThumbnailFront = "Resources /photo1.jpg", Artist = "", Title = fileName, isLocal = true });
            trackNo += 1;
            listView.ItemsSource = null;
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
            songIndex = songIndex - 1;
            if (songIndex > 0 && videos.Count > 0)
            {
                if (songIndex != System.Windows.Forms.ListBox.NoMatches)
                {
                    Trace.WriteLine(String.Format("Selected item is {0}", videos[songIndex].ThumbnailFront));


                }
                var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
                timer.Start();
                timer.Tick += (sender, args) =>
                {
                    try
                    {
                        if (!videos[songIndex].isLocal)
                        {
                            string songUrl = "https://www.youtube.com/watch?v=" + videos[songIndex].VideoID;
                            control.SourceProvider.MediaPlayer.Stop();
                            deleteAllTempFiles(Directory.GetCurrentDirectory() + @"\tmp\");
                            YoutubeBuffer(songUrl);
                            RenderSong();
                            listView.SelectedItem = listView.Items[songIndex];
                            Trace.WriteLine(String.Format("Done rendering!"));
                            timer.Stop();
                        }
                    }
                    catch (Exception e) {
                        Trace.WriteLine(String.Format("Index out of range! Error {0}", e));
                        control.SourceProvider.MediaPlayer.Stop();
                        PlayPauseIcon.Kind = PackIconKind.Stop;
                        EndTime.Text = "0:00:00";
                        StartTime.Text = "0:00:00";
                        slider.Value = 0;
                        this.isPlaying = false;
                    }
                   
                };
                try
                {
                    if (videos[songIndex].isLocal)
                    {

                        playLocalSongs(videos[songIndex].VideoID);

                    }
                }catch (Exception ex)
                {
                    Trace.WriteLine(String.Format("Index out of range! Error {0}", ex));
                    control.SourceProvider.MediaPlayer.Stop();
                    PlayPauseIcon.Kind = PackIconKind.Stop;
                    EndTime.Text = "0:00:00";
                    StartTime.Text = "0:00:00";
                    slider.Value = 0;
                    this.isPlaying = false;
                }
            }
        }



        private void Urmator_Click(object sender, RoutedEventArgs e)
        {
            songIndex = songIndex + 1;
            if (songIndex < listView.Items.Count && videos.Count > 0)
            {
                if (songIndex != System.Windows.Forms.ListBox.NoMatches)
                {
                    Trace.WriteLine(String.Format("Selected item is {0}", videos[songIndex].ThumbnailFront));


                }
                var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
                timer.Start();
                timer.Tick += (sender, args) =>
                {
                    try
                    {
                        if (!videos[songIndex].isLocal)
                        {
                            timer.Stop();
                            string songUrl = "https://www.youtube.com/watch?v=" + videos[songIndex].VideoID;
                            control.SourceProvider.MediaPlayer.Stop();
                            deleteAllTempFiles(Directory.GetCurrentDirectory() + @"\tmp\");
                            YoutubeBuffer(songUrl);
                            RenderSong();
                            listView.SelectedItem = listView.Items[songIndex];
                            Trace.WriteLine(String.Format("Done rendering!"));
                        }
                    }
                    catch (Exception e) {
                        Trace.WriteLine(String.Format("Index out of range! Error {0}", e));
                        control.SourceProvider.MediaPlayer.Stop();
                        PlayPauseIcon.Kind = PackIconKind.Stop;
                        EndTime.Text = "0:00:00";
                        StartTime.Text = "0:00:00";
                        slider.Value = 0;
                        this.isPlaying = false;

                    }
                   
                };
                try
                {
                    if (videos[songIndex].isLocal)
                    {

                        playLocalSongs(videos[songIndex].VideoID);

                    }
                }
                catch (Exception ex2)
                {
                    Trace.WriteLine(String.Format("Index out of range! Error {0}", ex2));
                    control.SourceProvider.MediaPlayer.Stop();
                    PlayPauseIcon.Kind = PackIconKind.Stop;
                    EndTime.Text = "0:00:00";
                    StartTime.Text = "0:00:00";
                    slider.Value = 0;
                    this.isPlaying = false;

                }
                
            }
        }

        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {
            button = sender as Button;
            pack = (button.FindName("PlayPauseIcon") as PackIcon);
            if (pack.Kind.Equals(PackIconKind.Play) && listView.Items.Count > 0)
            {
                this.isPlaying = true;
                (button.FindName("PlayPauseIcon") as PackIcon).Kind = PackIconKind.Pause;

                if (control.SourceProvider.MediaPlayer.Time > 0)
                {
                    control.SourceProvider.MediaPlayer.Play();
                }
                else
                {
                    try
                    {
                        songIndex = this.listView.Items.IndexOf(listView.SelectedItem);
                        if (!videos[songIndex].isLocal)
                        {
                            string songUrl = "https://www.youtube.com/watch?v=" + videos[songIndex].VideoID;
                            control.SourceProvider.MediaPlayer.Stop();
                            deleteAllTempFiles(Directory.GetCurrentDirectory() + @"\tmp\");
                            YoutubeBuffer(songUrl);
                            RenderSong();
                            Trace.WriteLine(String.Format("Done rendering!"));
                        }
                        else
                        {
                            playLocalSongs(videos[songIndex].VideoID);

                        }
                        InitTimer();
                    }
                    catch (Exception ex1) {
                        Trace.WriteLine(String.Format("Index out of range! Error {0}", ex1));
                        control.SourceProvider.MediaPlayer.Stop();
                        PlayPauseIcon.Kind = PackIconKind.Stop;
                        EndTime.Text = "0:00:00";
                        StartTime.Text = "0:00:00";
                        slider.Value = 0;
                        this.isPlaying = false;
                    }
                 }

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
            var uploadsListId = playlistId;

            var nextPageToken = "";
            

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
                        videos.Add(new Video() { TrackNo = trackNo, Name = playlistItem.Snippet.Title.ToString(), VideoID = playlistItem.Snippet.ResourceId.VideoId, Thumbnail = thumbnail, ThumbnailFront = Mainthumbnail, Artist = Artist, Title = Title, isLocal = false });
                        trackNo += 1;


                    }
                }

                nextPageToken = playlistItemsListResponse.NextPageToken;
            }



            Trace.WriteLine(String.Format("Videos:\n{0}\n", string.Join("\n", videos[0].Name)));



        }

        public void InitTimer()
        {
            Timer q = new Timer(1000);
            q.Elapsed += updateTimestampt;
            q.Start();
        }

    

        private void updateTimestampt(object sender, EventArgs e)
        {
            long startSliderTime = control.SourceProvider.MediaPlayer.Time;
            long endSliderTime = control.SourceProvider.MediaPlayer.Length;
            TimeSpan t_1 = TimeSpan.FromMilliseconds(control.SourceProvider.MediaPlayer.Time);
            String currentPlayTime = String.Format("{0:D1}:{1:D2}:{2:D2}",
                        t_1.Hours,
                        t_1.Minutes,
                        t_1.Seconds
                        );
           
            long totalTicks= control.SourceProvider.MediaPlayer.Length;
            TimeSpan t_2 = TimeSpan.FromMilliseconds(totalTicks);


            String totalTime = String.Format("{0:D1}:{1:D2}:{2:D2}",
                       t_2.Hours,
                       t_2.Minutes,
                       t_2.Seconds
                       );
           
            this.Dispatcher.Invoke(() =>
            {
                if (endSliderTime != 0)
                {
                    slider.Maximum = endSliderTime;
                    slider.Minimum = 0;
                    slider.Value = startSliderTime;
                    if (currentPlayTime.Equals(totalTime) && startSliderTime != -1)
                    {

                        PlayPauseIcon.Kind = PackIconKind.Play;
                        StartTime.Text = currentPlayTime;
                        EndTime.Text = "0:00:00";
                        StartTime.Text = "0:00:00";
                        slider.Value = 0;
                        if (listView.Items.Count > 0)
                        {
                            songIndex = listView.SelectedIndex;
                        }
                        autoPlayNextSong();

                    }
                    if (videos.Count > 0)
                    {
                        try
                        {
                            if (videos[songIndex].isLocal)
                            {
                                if (TimeSpan.FromMilliseconds(control.SourceProvider.MediaPlayer.Length) - TimeSpan.FromSeconds(1) == TimeSpan.FromMilliseconds(control.SourceProvider.MediaPlayer.Time))
                                {
                                    if (listView.Items.Count > 0)
                                    {
                                        songIndex = listView.SelectedIndex;
                                    }
                                    autoPlayNextSong();
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Trace.WriteLine(String.Format("Index out of range! Error {0}", e));
                            control.SourceProvider.MediaPlayer.Stop();
                            PlayPauseIcon.Kind = PackIconKind.Stop;
                            EndTime.Text = "0:00:00";
                            StartTime.Text = "0:00:00";
                            slider.Value = 0;
                            this.isPlaying = false;

                        }

                    }
                    if (this.isPlaying)
                    {
                        StartTime.Text = currentPlayTime;
                        EndTime.Text = totalTime;
                    }
                    else
                    {
                        EndTime.Text = "0:00:00";
                        StartTime.Text = "0:00:00";
                        slider.Value = 0;
                    }

                }
            }); 
       

            Trace.WriteLine(String.Format("Time is {0} and total time is {1} , timestampt start {2}, timestampt end {3}",currentPlayTime,totalTime,startSliderTime,endSliderTime));
        }

        private void autoPlayNextSong() {
            if (listView.Items.Count > 0)
            {
                songIndex = songIndex + 1;
                if (songIndex < listView.Items.Count && !control.SourceProvider.MediaPlayer.IsPlaying())
                {
                    if (songIndex != System.Windows.Forms.ListBox.NoMatches)
                    {
                        Trace.WriteLine(String.Format("Selected item is {0}", videos[songIndex].ThumbnailFront));


                    }
                    try
                    {
                        if (!videos[songIndex].isLocal)
                        {
                            string songUrl = "https://www.youtube.com/watch?v=" + videos[songIndex].VideoID;
                            control.SourceProvider.MediaPlayer.Stop();
                            deleteAllTempFiles(Directory.GetCurrentDirectory() + @"\tmp\");
                            YoutubeBuffer(songUrl);
                            RenderSong();
                            Trace.WriteLine(String.Format("Done rendering!"));
                        }
                        else
                        {
                            playLocalSongs(videos[songIndex].VideoID);

                        }
                    }
                    catch (Exception e) {

                        Trace.WriteLine(String.Format("Index out of range! Error {0}", e));
                        control.SourceProvider.MediaPlayer.Stop();
                        PlayPauseIcon.Kind = PackIconKind.Stop;
                        EndTime.Text = "0:00:00";
                        StartTime.Text = "0:00:00";
                        slider.Value = 0;
                        this.isPlaying = false;

                    }
                }
            }
            

        }


        private void playLocalSongs(String url)
        {
            if (control.SourceProvider.MediaPlayer.IsPlaying())
            {
                control.SourceProvider.MediaPlayer.Stop();
                PlayPauseIcon.Kind = PackIconKind.Pause;
            }
            control.SourceProvider.MediaPlayer.Play(new Uri(url));
            PlayPauseIcon.Kind = PackIconKind.Pause;
            Thumbnail.ImageSource = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + @"\Resources\photo1.jpg"));
            Artist.Text = videos[songIndex].Artist;
            Title.Text = videos[songIndex].Title;
            listView.SelectedItem = listView.Items[songIndex];
            this.isPlaying = true;

        }

        private async Task RenderSong()
        {
            PlayPauseIcon.Kind = PackIconKind.Play;

            await Task.Delay(5500);
            if (control.SourceProvider.MediaPlayer.IsPlaying())
            {
                control.SourceProvider.MediaPlayer.Stop();
                control.SourceProvider.MediaPlayer.Play(new Uri(Directory.GetCurrentDirectory() + @"\tmp\temp.m4a"));
                
            }
            control.SourceProvider.MediaPlayer.Play(new Uri(Directory.GetCurrentDirectory() + @"\tmp\temp.m4a"));
            PlayPauseIcon.Kind = PackIconKind.Pause;
            Thumbnail.ImageSource = new BitmapImage(new Uri(videos[songIndex].ThumbnailFront.ToString()));
            Artist.Text = videos[songIndex].Artist;
            Title.Text = videos[songIndex].Title;
            listView.SelectedItem = listView.Items[songIndex];
            this.isPlaying = true;
        }

            private void listView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            songIndex = this.listView.Items.IndexOf(listView.SelectedItem);
            try
            {
                if (!videos[songIndex].isLocal)
                {
                    string songUrl = "https://www.youtube.com/watch?v=" + videos[songIndex].VideoID;
                    control.SourceProvider.MediaPlayer.Stop();
                    deleteAllTempFiles(Directory.GetCurrentDirectory() + @"\tmp\");
                    YoutubeBuffer(songUrl);
                    RenderSong();
                    Trace.WriteLine(String.Format("Done rendering!"));
                }
                else
                {
                    playLocalSongs(videos[songIndex].VideoID);

                }
            }catch(Exception ex3)
            {
                Trace.WriteLine(String.Format("Index out of range! Error {0}", ex3));
                control.SourceProvider.MediaPlayer.Stop();
                PlayPauseIcon.Kind = PackIconKind.Stop;
                EndTime.Text = "0:00:00";
                StartTime.Text = "0:00:00";
                slider.Value = 0;
                this.isPlaying = false;

            }
            
             InitTimer();
                    
           
        }


        private void deleteAllTempFiles(String url) {

            DirectoryInfo di = new DirectoryInfo(url);
            foreach (FileInfo file in di.GetFiles())
            {
                control.SourceProvider.MediaPlayer.Stop();
                try
                {
                    file.Delete();
                }catch(Exception e)
                {
                    Trace.WriteLine("File does not exists! Error{0}",e.ToString());
                }
            }
        }

        private void slider_seek(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            long timeSeek = (long)slider.Value;
            control.SourceProvider.MediaPlayer.Time = timeSeek;
            Trace.WriteLine(String.Format("Seeked time {0}", slider.Value));
        }

        private void clearList(object sender, RoutedEventArgs e)
        {
            videos.Clear();
            listView.ItemsSource = null;
            trackNo = 0;
            songIndex = 0;
        }
    }
}








