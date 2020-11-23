using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AlbumImageSearch.Configuration;
using AlbumImageSearch.Cosmos;
using AlbumImageSearch.Search;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Reverb;

namespace AlbumImageSearch
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<SpotifyOptions>(Configuration.GetSection(SpotifyOptions.SectionName));
            services.Configure<ComputerVisionOptions>(Configuration.GetSection(ComputerVisionOptions.SectionName));
            services.Configure<CosmosOptions>(Configuration.GetSection(CosmosOptions.SectionName));
            services.Configure<SearchOptions>(Configuration.GetSection(SearchOptions.SectionName));

            services.AddScoped<SpotifyClient>(s =>
            {
                IOptions<SpotifyOptions> spotifyOptions = s.GetRequiredService<IOptions<SpotifyOptions>>();
                return new SpotifyClient(spotifyOptions.Value.ClientId, spotifyOptions.Value.ClientSecret, spotifyOptions.Value.RedirectUrl);
            });
            services.AddSingleton<HttpClient>();
            services.AddSingleton<ComputerVisionAPI>(s =>
            {
                IOptions<ComputerVisionOptions> computerVisionOptions = s.GetRequiredService<IOptions<ComputerVisionOptions>>();
                HttpClient httpClient = s.GetRequiredService<HttpClient>();
                return new ComputerVisionAPI(computerVisionOptions, httpClient);
            });
            services.AddSingleton<CosmosAPI>(s =>
            {
                IOptions<CosmosOptions> cosmosOptions = s.GetRequiredService<IOptions<CosmosOptions>>();
                CosmosAPI cosmosAPI = new CosmosAPI(cosmosOptions);
                // ensure CosmosAPI is setup before startup
                // Service setup can't be async so use this...
                cosmosAPI.Setup().GetAwaiter().GetResult();
                return cosmosAPI;
            });
            services.AddSingleton<SearchAPI>(s =>
            {
                IOptions<SearchOptions> searchOptions = s.GetRequiredService<IOptions<SearchOptions>>();
                SearchAPI searchAPI = new SearchAPI(searchOptions);
                return searchAPI;
            });
            services.AddSingleton<HelperMethods>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "AlbumImageSearch", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AlbumImageSearch v1"));
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseCors(builder =>
                builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
        }
    }
}
