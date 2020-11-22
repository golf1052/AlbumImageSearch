using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlbumImageSearch.Configuration;
using Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace AlbumImageSearch.Cosmos
{
    public class CosmosAPI
    {
        private const string AlbumsDatabaseId = "Albums";
        private const string AlbumsContainerId = "Albums";
        private CosmosClient cosmosClient;
        private CosmosDatabase? albumsDatabase;
        private CosmosContainer? albumsContainer;

        public CosmosAPI(IOptions<CosmosOptions> options)
        {
            cosmosClient = new CosmosClient(options.Value.Endpoint, options.Value.AccessKey);
        }

        public async Task Setup()
        {
            albumsDatabase = await cosmosClient.CreateDatabaseIfNotExistsAsync(AlbumsDatabaseId);
            albumsContainer = await albumsDatabase.CreateContainerIfNotExistsAsync(AlbumsContainerId, "/id");
        }

        public async Task SaveAlbumInfo(AlbumInfo albumInfo)
        {
            string albumInfoString = albumInfo.ToString();
            try
            {
                ItemResponse<AlbumInfo> response = await albumsContainer!.UpsertItemAsync<AlbumInfo>(albumInfo, new PartitionKey(albumInfo.AlbumId));
            }
            catch (CosmosException ex)
            {
                throw;
            }
        }

        public async Task<AlbumInfo?> GetAlbumInfo(string albumId)
        {
            try
            {
                ItemResponse<AlbumInfo> response = await albumsContainer!.ReadItemAsync<AlbumInfo>(albumId, new PartitionKey(albumId));
                return response.Value;
            }
            catch (CosmosException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
