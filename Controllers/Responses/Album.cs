using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlbumImageSearch.Controllers.Responses
{
    public class Album
    {
        public string? AlbumId { get; set; }
        public string? AlbumName { get; set; }
        public string? ArtistName { get; set; }
        public string? ImageUrl { get; set; }
    }
}
