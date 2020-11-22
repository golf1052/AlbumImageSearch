using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlbumImageSearch.Controllers.Responses
{
    public class ProcessAlbumsResponse
    {
        public List<Album> Albums { get; set; }

        public ProcessAlbumsResponse()
        {
            Albums = new List<Album>();
        }
    }
}
