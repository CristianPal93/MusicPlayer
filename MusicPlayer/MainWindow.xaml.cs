﻿using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
    

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                var currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                // Default installation path of VideoLAN.LibVLC.Windows
                var libDirectory =
                    new DirectoryInfo(".\\libvlc\\");
                Trace.WriteLine(String.Format("path {0}", libDirectory));

                using (var mediaPlayer = new Vlc.DotNet.Core.VlcMediaPlayer(libDirectory))
                {

                    var mediaOptions = new[]
                    {
                    ":sout=#rtp{sdp=rtsp://127.0.0.1:554/}",
                    ":sout-keep"
                };

                    mediaPlayer.SetMedia(new Uri("http://hls1.addictradio.net/addictrock_aac_hls/playlist.m3u8"),
                        mediaOptions);

                    mediaPlayer.Play();

                    Console.WriteLine("Streaming on rtsp://127.0.0.1:554/");
                    Console.WriteLine("Press any key to exit");
                    

                }
            }

            catch (System.Windows.Markup.XamlParseException e)
            {

                Console.WriteLine(e);
            }
            //  Console.ReadKey();
        }


        public  async Task SyncList()
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
            Environment.Exit(0);

        }

        private void WindowMinimized(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;

        }

        private void WindowMaximized(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            PackIcon pack = (button.FindName("WindowSettings") as PackIcon);
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
            Button button = sender as Button;
            PackIcon pack = (button.FindName("PlayPauseIcon") as PackIcon);
            if (pack.Kind.Equals(PackIconKind.Play))
            {
                this.isPlaying = true;
                (button.FindName("PlayPauseIcon") as PackIcon).Kind = PackIconKind.Pause;

            }
            else
            {

                this.isPlaying = false;
                (button.FindName("PlayPauseIcon") as PackIcon).Kind = PackIconKind.Play;

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
        public  async Task GetYoutubePlayList(string playlistId)
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
                    if (playlistItem.Snippet.Title.ToString() != "Deleted video" || playlistItem.Snippet.Title.ToString() != "Private video" )
                    {
                        // Print information about each video.
                        String thumbnail = "https://img.youtube.com/vi/" + playlistItem.Snippet.ResourceId.VideoId.ToString() + "/default.jpg";
                        videos.Add(new Video() { TrackNo=trackNo,Name=playlistItem.Snippet.Title.ToString(),videoID=playlistItem.Snippet.ResourceId.VideoId,Thumbnail=thumbnail});
                        trackNo += 1;


                    }
                }

                nextPageToken = playlistItemsListResponse.NextPageToken;
            }



            Trace.WriteLine(String.Format("Videos:\n{0}\n", string.Join("\n", videos[0].Name)));
           


        }



       

    }
}








