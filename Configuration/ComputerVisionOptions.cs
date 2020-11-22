using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlbumImageSearch.Configuration
{
    public class ComputerVisionOptions
    {
        public const string SectionName = "ComputerVision";

        public string? AccessKey { get; set; }
        public string? Endpoint { get; set; }
    }
}
