using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AlbumImageSearch.Cosmos
{
    public class AlbumInfo
    {
        [JsonPropertyName("id")]
        public string? AlbumId { get; set; }
        public string? AlbumName { get; set; }
        public string? ArtistName { get; set; }
        public VisionResults? VisionResults { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
