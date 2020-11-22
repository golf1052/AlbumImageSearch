using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlbumImageSearch.Controllers.Responses
{
    public class SearchResponse
    {
        public List<string> AlbumIds { get; set; }

        public SearchResponse()
        {
            AlbumIds = new List<string>();
        }
    }
}
