using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Reverb;
using Reverb.Models;

namespace AlbumImageSearch.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpotifyController : ControllerBase
    {
        private HttpClient httpClient;
        private SpotifyClient spotifyClient;

        public SpotifyController(HttpClient httpClient, SpotifyClient spotifyClient)
        {
            this.httpClient = httpClient;
            this.spotifyClient = spotifyClient;
        }

        [HttpGet("authorize")]
        public string GetAuthorize()
        {
            List<SpotifyConstants.SpotifyScopes> scopes = new List<SpotifyConstants.SpotifyScopes>();
            scopes.Add(SpotifyConstants.SpotifyScopes.UserLibraryRead);
            return spotifyClient.GetAuthorizeUrl(scopes);
        }

        [HttpPost("authorize")]
        public async Task<string> PostAuthorize([FromForm]string code)
        {
            return await spotifyClient.RequestAccessToken(code);
        }

        [HttpPost("ProcessAlbums")]
        public async Task ProcessAlbums([FromForm]string accessToken)
        {
            spotifyClient.AccessToken = accessToken;
            int total = 0;
            int offset = 0;
            List<SpotifySavedAlbum> albums = new List<SpotifySavedAlbum>();
            do
            {
                int limit = 50;
                var savedAlbums = await spotifyClient.GetUserSavedAlbums(limit, offset);
                albums.AddRange(savedAlbums.Items);
                total = savedAlbums.Total;
            }
            while (albums.Count < total);

            foreach (var album in albums)
            {
                SpotifyImage largestImage = album.Album.GetLargestImage();
                if (largestImage == null || largestImage.Url == null)
                {
                    continue;
                }
                HttpResponseMessage responseMessage = await httpClient.GetAsync(largestImage.Url);
                if (!responseMessage.IsSuccessStatusCode)
                {
                    string responseString = await responseMessage.Content.ReadAsStringAsync();
                    throw new Exception(responseString);
                }
                byte[] imageByteData = await responseMessage.Content.ReadAsByteArrayAsync();
                System.IO.Directory.CreateDirectory("albums");
                System.IO.File.WriteAllBytes($"albums/{album.Album.Name}.jpg", imageByteData);
            }
        }
    }
}
