using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace youtube_mp3_video_converter.Models
{
    public class youtubeContext : DbContext
    {
        public DbSet<ConvertedVideo> ConvertedVideo { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ConvertedVideo>();
        }
    }

    public class ConvertedVideo
    {
        [Key]
        public int youtubeId { get; set; }
        public string videoId { get; set; }
        public string thumbnail_url { get; set; }
        public string title { get; set; }
        public DateTime convertDate { get; set; }
    }
}