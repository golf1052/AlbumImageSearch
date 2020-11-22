using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlbumImageSearch.Configuration
{
    public class CosmosOptions
    {
        public const string SectionName = "Cosmos";

        public string? AccessKey { get; set; }
        public string? ConnectionString { get; set; }
        public string? Endpoint { get; set; }
    }
}
