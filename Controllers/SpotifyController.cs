using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using AlbumImageSearch.Configuration;
using AlbumImageSearch.Cosmos;
using AlbumImageSearch.Search;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Reverb;
using Reverb.Models;
using Newtonsoft.Json;
using AlbumImageSearch.Controllers.Responses;
using Azure.Search.Documents.Models;
using AlbumImageSearch.Controllers.Requests;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AlbumImageSearch.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpotifyController : ControllerBase
    {
        private HttpClient httpClient;
        private SpotifyClient spotifyClient;
        private HelperMethods helperMethods;
        private ComputerVisionAPI computerVisionAPI;
        private CosmosAPI cosmosAPI;
        private SearchAPI searchAPI;

        public SpotifyController(HttpClient httpClient,
            SpotifyClient spotifyClient,
            HelperMethods helperMethods,
            ComputerVisionAPI computerVisionAPI,
            CosmosAPI cosmosAPI,
            SearchAPI searchAPI)
        {
            this.httpClient = httpClient;
            this.spotifyClient = spotifyClient;
            this.helperMethods = helperMethods;
            this.computerVisionAPI = computerVisionAPI;
            this.cosmosAPI = cosmosAPI;
            this.searchAPI = searchAPI;
        }

        [HttpGet("authorize")]
        public string GetAuthorize()
        {
            List<SpotifyConstants.SpotifyScopes> scopes = new List<SpotifyConstants.SpotifyScopes>();
            scopes.Add(SpotifyConstants.SpotifyScopes.UserLibraryRead);
            return spotifyClient.GetAuthorizeUrl(scopes, helperMethods.GetRandomString(32));
        }

        [HttpGet("/redirect")]
        public async Task<IActionResult> Redirect([FromQuery][Required] string code,
            [FromQuery][Required] string state,
            [FromQuery] string? error = null)
        {
            if (!string.IsNullOrEmpty(error))
            {
                return LocalRedirect($"/#error={error}");
            }
            string accessToken = await spotifyClient.RequestAccessToken(code);
            var profile = await spotifyClient.GetCurrentUserProfile();
            UserInfo userInfo = new UserInfo()
            {
                UserId = profile.Id,
                State = state
            };
            await cosmosAPI.SaveUserInfo(userInfo);
            return LocalRedirect($"/#accessToken={accessToken}&state={state}");
        }

        [HttpPost("ProcessAlbums")]
        public async Task<ProcessAlbumsResponse> ProcessAlbums([FromForm][Required] string accessToken,
            [FromForm][Required] string state)
        {
            if (!await AuthenticateUser(accessToken, state))
            {
                Response.StatusCode = StatusCodes.Status401Unauthorized;
                return new ProcessAlbumsResponse();
            }

            int total = 0;
            int offset = 0;
            List<SpotifySavedAlbum> albums = new List<SpotifySavedAlbum>();
            do
            {
                int limit = 50;
                var savedAlbums = await spotifyClient.GetUserSavedAlbums(limit, offset);
                offset += limit;
                albums.AddRange(savedAlbums.Items);
                total = savedAlbums.Total;
            }
            while (albums.Count < total);

            ProcessAlbumsResponse response = new ProcessAlbumsResponse();

            foreach (var album in albums)
            {
                SpotifyImage largestImage = album.Album.GetLargestImage();
                if (largestImage == null || largestImage.Url == null)
                {
                    continue;
                }

                AlbumInfo? albumInfo = await cosmosAPI.GetAlbumInfo(album.Album.Uri);
                if (albumInfo == null)
                {
                    ImageAnalysis results;
                    try
                    {
                        results = await computerVisionAPI.AnalyzeImage(largestImage.Url);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                        continue;
                    }

                    string? artist = null;
                    if (album.Album.Artists.Count > 0)
                    {
                        artist = album.Album.Artists[0].Name;
                    }
                    albumInfo = new AlbumInfo()
                    {
                        AlbumId = album.Album.Uri,
                        AlbumName = album.Album.Name,
                        ArtistName = artist,
                        VisionResults = new VisionResults()
                        {
                            Color = results.Color,
                            Description = results.Description,
                            ImageType = results.ImageType,
                            Tags = results.Tags
                        }
                    };

                    try
                    {
                        await cosmosAPI.SaveAlbumInfo(albumInfo);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                        continue;
                    }

                    AlbumInfoSearchObject searchObject = albumInfo.ToSearchObject();

                    try
                    {
                        await searchAPI.UploadDocument(searchObject);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                    }
                }

                response.Albums.Add(album.ToResponseAlbum());
            }

            return response;
        }

        [HttpPost("search")]
        public async Task<SearchResponse> Search([FromBody] SearchRequest request)
        {
            if (!await AuthenticateUser(request.AccessToken!, request.State!))
            {
                Response.StatusCode = StatusCodes.Status401Unauthorized;
                return new SearchResponse();
            }

            var results = await searchAPI.Search(request.SearchQuery!);
            HashSet<string> albumIdsSet = new HashSet<string>(request.AlbumIds!);

            SearchResponse response = new SearchResponse();
            await foreach (SearchResult<AlbumInfoSearchObject> result in results.GetResultsAsync())
            {
                string fullId = $"spotify:album:{result.Document.AlbumId!}";
                if (albumIdsSet.Contains(fullId))
                {
                    response.AlbumIds.Add(fullId);
                }
            }

            return response;
        }

        private async Task<bool> AuthenticateUser(string accessToken, string state)
        {
            spotifyClient.AccessToken = accessToken;

            SpotifyCurrentUserProfile profile;
            try
            {
                profile = await spotifyClient.GetCurrentUserProfile();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                throw;
            }

            UserInfo? userInfo = await cosmosAPI.GetUserInfo(state, profile.Id);
            if (userInfo == null)
            {
                return false;
            }
            else
            {
                return userInfo.UserId == profile.Id;
            }
        }
    }
}
