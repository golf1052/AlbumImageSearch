using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlbumImageSearch.Configuration
{
    public class SpotifyOptions
    {
        public const string SectionName = "Spotify";

        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string? RedirectUrl { get; set; }
    }
}
