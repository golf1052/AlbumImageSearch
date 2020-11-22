using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlbumImageSearch.Configuration
{
    public class SearchOptions
    {
        public const string SectionName = "Search";

        public string? AccessKey { get; set; }
        public string? Endpoint { get; set; }
    }
}
