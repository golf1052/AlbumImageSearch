using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlbumImageSearch.Controllers.Responses;
using AlbumImageSearch.Cosmos;
using AlbumImageSearch.Search;
using Reverb.Models;

namespace AlbumImageSearch
{
    public static class ExtensionMethods
    {
        public static AlbumInfoSearchObject ToSearchObject(this AlbumInfo albumInfo)
        {
            AlbumInfoSearchObject searchObject = new AlbumInfoSearchObject()
            {
                AlbumId = albumInfo.AlbumId!.Split(':')[2],
                AlbumName = albumInfo.AlbumName,
                ArtistName = albumInfo.ArtistName,
                Captions = new List<string>()
            };

            HashSet<string> colors = new HashSet<string>();
            colors.Add(albumInfo.VisionResults!.Color!.DominantColorBackground);
            colors.Add(albumInfo.VisionResults!.Color!.DominantColorForeground);
            foreach (var color in albumInfo.VisionResults!.Color.DominantColors)
            {
                colors.Add(color);
            }
            searchObject.Colors = colors.ToList();

            HashSet<string> tags = new HashSet<string>();
            foreach (var tag in albumInfo.VisionResults!.Tags!)
            {
                tags.Add(tag.Name);
            }

            foreach (var tag in albumInfo.VisionResults!.Description!.Tags)
            {
                tags.Add(tag);
            }
            searchObject.Tags = tags.ToList();

            foreach (var caption in albumInfo.VisionResults!.Description!.Captions)
            {
                searchObject.Captions.Add(caption.Text);
            }
            return searchObject;
        }

        public static Album ToResponseAlbum(this SpotifySavedAlbum savedAlbum)
        {
            string? artistName = null;
            if (savedAlbum.Album.Artists.Count > 0)
            {
                artistName = savedAlbum.Album.Artists[0].Name;
            }

            Album album = new Album()
            {
                AlbumId = savedAlbum.Album.Uri,
                AlbumName = savedAlbum.Album.Name,
                ArtistName = artistName,
                ImageUrl = savedAlbum.Album.GetLargestImage()?.Url
            };

            return album;
        }
    }
}
