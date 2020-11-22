using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlbumImageSearch.Controllers.Requests
{
    public class SearchRequest
    {
        public string? AccessToken { get; set; }
        public List<string>? AlbumIds { get; set; }
        public string? SearchQuery { get; set; }
    }
}
