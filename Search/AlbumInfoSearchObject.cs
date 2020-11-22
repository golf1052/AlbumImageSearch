using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

namespace AlbumImageSearch.Search
{
    public class AlbumInfoSearchObject
    {
        [SimpleField(IsKey = true)]
        public string? AlbumId { get; set; }

        [SearchableField]
        public string? AlbumName { get; set; }

        [SearchableField]
        public string? ArtistName { get; set; }

        [SearchableField]
        public IList<string>? Colors { get; set; }

        [SearchableField]
        public IList<string>? Tags { get; set; }

        [SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EnMicrosoft)]
        public IList<string>? Captions { get; set; }
    }
}
