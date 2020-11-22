using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlbumImageSearch.Configuration;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Options;

namespace AlbumImageSearch.Search
{
    public class SearchAPI
    {
        private const string IndexName = "albums";
        private SearchIndexClient searchIndexClient;
        private SearchClient searchClient;

        public SearchAPI(IOptions<Configuration.SearchOptions> options)
        {
            Uri endpointUri = new Uri(options.Value.Endpoint!);
            Azure.AzureKeyCredential credentials = new Azure.AzureKeyCredential(options.Value.AccessKey!);
            searchIndexClient = new SearchIndexClient(endpointUri, credentials);
            searchClient = new SearchClient(endpointUri, IndexName, credentials);

            FieldBuilder fieldBuilder = new FieldBuilder();
            var searchFields = fieldBuilder.Build(typeof(AlbumInfoSearchObject));

            var definition = new SearchIndex(IndexName, searchFields);
            var suggester = new SearchSuggester("AlbumsSuggester",
                new[] { nameof(AlbumInfoSearchObject.AlbumName),
                    nameof(AlbumInfoSearchObject.ArtistName),
                    nameof(AlbumInfoSearchObject.Colors),
                    nameof(AlbumInfoSearchObject.Tags),
                    nameof(AlbumInfoSearchObject.Captions) });
            definition.Suggesters.Add(suggester);
            searchIndexClient.CreateOrUpdateIndex(definition);
        }

        public async Task UploadDocument(AlbumInfoSearchObject document)
        {
            IndexDocumentsBatch<AlbumInfoSearchObject> batch = IndexDocumentsBatch.Create(IndexDocumentsAction.MergeOrUpload(document));

            try
            {
                IndexDocumentsResult result = await searchClient.IndexDocumentsAsync(batch);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<SearchResults<AlbumInfoSearchObject>> Search(string searchQuery)
        {
            SearchResults<AlbumInfoSearchObject> response = await searchClient.SearchAsync<AlbumInfoSearchObject>(searchQuery);
            return response;
        }
    }
}
