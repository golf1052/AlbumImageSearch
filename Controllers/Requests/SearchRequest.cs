using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AlbumImageSearch.Controllers.Requests
{
    public class SearchRequest
    {
        [Required]
        public string? AccessToken { get; set; }

        [Required]
        public string? State { get; set; }

        [Required]
        public List<string>? AlbumIds { get; set; }

        [Required]
        public string? SearchQuery { get; set; }
    }
}
