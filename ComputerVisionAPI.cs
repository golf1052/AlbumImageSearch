using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AlbumImageSearch.Configuration;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Options;

namespace AlbumImageSearch
{
    public class ComputerVisionAPI
    {
        private ComputerVisionClient computerVisionClient;

        public ComputerVisionAPI(IOptions<ComputerVisionOptions> options, HttpClient httpClient)
        {
            computerVisionClient = new ComputerVisionClient(new ApiKeyServiceClientCredentials(options.Value.AccessKey), httpClient, false);
            computerVisionClient.Endpoint = options.Value.Endpoint;
        }

        public async Task<ImageAnalysis> AnalyzeImage(string url)
        {
            List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>()
            {
                VisualFeatureTypes.Tags,
                VisualFeatureTypes.Description,
                VisualFeatureTypes.Color,
                VisualFeatureTypes.ImageType
            };

            ImageAnalysis? results = null;
            do
            {
                try
                {
                    results = await computerVisionClient.AnalyzeImageAsync(url, features);
                    return results;
                }
                catch (ComputerVisionErrorException ex)
                {
                    if (ex.Response.Headers.ContainsKey("Retry-After"))
                    {
                        long seconds = long.Parse(ex.Response.Headers["Retry-After"].First());
                        await Task.Delay(TimeSpan.FromSeconds(seconds));
                        continue;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            while (results == null);

            return results;
        }
    }
}
