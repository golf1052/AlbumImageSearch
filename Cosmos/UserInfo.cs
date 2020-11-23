using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AlbumImageSearch.Cosmos
{
    public class UserInfo
    {
        [JsonPropertyName("id")]
        public string? State { get; set; }
        public string? UserId { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
