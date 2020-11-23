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

        private const string UsersDatabaseId = "Users";
        private const string UsersContainerId = "Users";

        private CosmosClient cosmosClient;

        private CosmosDatabase? albumsDatabase;
        private CosmosContainer? albumsContainer;

        private CosmosDatabase? usersDatabase;
        private CosmosContainer? usersContainer;

        public CosmosAPI(IOptions<CosmosOptions> options)
        {
            cosmosClient = new CosmosClient(options.Value.Endpoint, options.Value.AccessKey);
        }

        public async Task Setup()
        {
            albumsDatabase = await cosmosClient.CreateDatabaseIfNotExistsAsync(AlbumsDatabaseId);
            albumsContainer = await albumsDatabase.CreateContainerIfNotExistsAsync(AlbumsContainerId, "/id");

            usersDatabase = await cosmosClient.CreateDatabaseIfNotExistsAsync(UsersDatabaseId);
            ContainerProperties usersContainerProperties = new ContainerProperties(UsersContainerId, "/UserId")
            {
                DefaultTimeToLive = 3600
            };
            usersContainer = (await usersDatabase.CreateContainerIfNotExistsAsync(usersContainerProperties)).Container;
        }

        public async Task SaveAlbumInfo(AlbumInfo albumInfo)
        {
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
                return null;
            }
        }

        public async Task SaveUserInfo(UserInfo userInfo)
        {
            try
            {
                ItemResponse<UserInfo> response = await usersContainer!.UpsertItemAsync<UserInfo>(userInfo, new PartitionKey(userInfo.UserId));
            }
            catch (CosmosException ex)
            {
                throw;
            }
        }

        public async Task<UserInfo?> GetUserInfo(string state, string userId)
        {
            try
            {
                ItemResponse<UserInfo> response = await usersContainer!.ReadItemAsync<UserInfo>(state, new PartitionKey(userId));
                return response.Value;
            }
            catch (CosmosException ex)
            {
                return null;
            }
        }
    }
}
