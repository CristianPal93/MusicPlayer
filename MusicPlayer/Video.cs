using System;
using System.Collections.Generic;
using System.Text;

namespace MusicPlayer
{
    class Video
    {   
        public int TrackNo { get; set; }
        public string Name { get; set; }

        public string VideoID { get;  set; }

        public string Thumbnail { get; set; }

        public string ThumbnailFront { get; set; }
        
        public string Artist { set; get; }

        public string Title { set; get; }

        public bool isLocal { set; get; }

    }
}
