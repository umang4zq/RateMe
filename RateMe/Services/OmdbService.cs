using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using RateMe.Models;

namespace RateMe.Services
{
    public class OmdbService
    {
        private static readonly HttpClient _http = new HttpClient();
        private static readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();
        private string _apiKey = "cf367844";

        public string ApiKey
        {
            get => _apiKey;
            set => _apiKey = value;
        }

        public OmdbService(string apiKey = "cf367844")
        {
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                _apiKey = apiKey;
            }
        }

        public async Task<List<MediaItem>> SearchAsync(string query, string type = null, int page = 1)
        {
            var results = new List<MediaItem>();
            if (string.IsNullOrWhiteSpace(query))
                return results;

            try
            {
                string url = $"http://www.omdbapi.com/?s={Uri.EscapeDataString(query)}&page={page}&apikey={_apiKey}";
                if (!string.IsNullOrEmpty(type) && !type.Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    url += $"&type={type.ToLowerInvariant()}";
                }

                string json = await _http.GetStringAsync(url).ConfigureAwait(false);
                var response = _serializer.Deserialize<OmdbSearchResponse>(json);

                if (response != null && response.Search != null)
                {
                    foreach (var item in response.Search)
                    {
                        results.Add(new MediaItem
                        {
                            ImdbId = item.imdbID,
                            Title = item.Title,
                            Year = item.Year,
                            Type = item.Type,
                            PosterUrl = item.Poster
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("OMDb search error: " + ex.Message);
            }

            return results;
        }

        public async Task<MediaItem> GetDetailByIdAsync(string imdbId)
        {
            if (string.IsNullOrWhiteSpace(imdbId))
                return null;

            try
            {
                string url = $"http://www.omdbapi.com/?i={Uri.EscapeDataString(imdbId)}&plot=full&apikey={_apiKey}";
                string json = await _http.GetStringAsync(url).ConfigureAwait(false);
                var detail = _serializer.Deserialize<OmdbDetailResponse>(json);

                if (detail != null && string.Equals(detail.Response, "True", StringComparison.OrdinalIgnoreCase))
                {
                    return ConvertDetailToMediaItem(detail);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("OMDb detail error: " + ex.Message);
            }

            return null;
        }

        public async Task<MediaItem> GetDetailByTitleAsync(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return null;

            try
            {
                string url = $"http://www.omdbapi.com/?t={Uri.EscapeDataString(title)}&plot=full&apikey={_apiKey}";
                string json = await _http.GetStringAsync(url).ConfigureAwait(false);
                var detail = _serializer.Deserialize<OmdbDetailResponse>(json);

                if (detail != null && string.Equals(detail.Response, "True", StringComparison.OrdinalIgnoreCase))
                {
                    return ConvertDetailToMediaItem(detail);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("OMDb detail by title error: " + ex.Message);
            }

            return null;
        }

        public async Task<bool> TestApiKeyAsync(string key)
        {
            try
            {
                string url = $"http://www.omdbapi.com/?i=tt3896198&apikey={key}";
                string json = await _http.GetStringAsync(url).ConfigureAwait(false);
                var detail = _serializer.Deserialize<OmdbDetailResponse>(json);
                return detail != null && string.Equals(detail.Response, "True", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private MediaItem ConvertDetailToMediaItem(OmdbDetailResponse detail)
        {
            decimal? imdbScore = null;
            if (decimal.TryParse(detail.imdbRating, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsedScore))
            {
                imdbScore = parsedScore;
            }

            int? metascore = null;
            if (int.TryParse(detail.Metascore, out int parsedMeta))
            {
                metascore = parsedMeta;
            }

            return new MediaItem
            {
                ImdbId = detail.imdbID,
                Title = detail.Title,
                Year = detail.Year,
                Type = detail.Type ?? "movie",
                Genre = detail.Genre,
                Director = detail.Director,
                Actors = detail.Actors,
                Plot = detail.Plot,
                PosterUrl = detail.Poster,
                ImdbRating = imdbScore,
                Metascore = metascore,
                Runtime = detail.Runtime
            };
        }
    }
}

