using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace AlbumImageSearch.Cosmos
{
    public class VisionResults
    {
        public ColorInfo? Color { get; set; }
        public ImageType? ImageType { get; set; }
        public IList<ImageTag>? Tags { get; set; }
        public ImageDescriptionDetails? Description { get; set; }
    }
}
