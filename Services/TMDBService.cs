using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using RedFlix.Models.Tmdb;

namespace RedFlix.Services
{
    public class TMDBService
    {
        private const string BaseUrl = "https://api.themoviedb.org/3/";
        private const string ImageBaseUrl = "https://image.tmdb.org/t/p/w500";
        private static readonly HttpClient Client = new HttpClient();

        private readonly string _apiKey;

        public TMDBService()
        {
            _apiKey = ConfigurationManager.AppSettings["TmdbApiKey"];
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                throw new ConfigurationErrorsException("Falta la clave TmdbApiKey en Web.config.");
            }
        }

        public static string GetPosterUrl(string posterPath)
        {
            return string.IsNullOrWhiteSpace(posterPath)
                ? "https://via.placeholder.com/300x450?text=Sin+poster"
                : ImageBaseUrl + posterPath;
        }

        public static string GetBackdropUrl(string backdropPath)
        {
            return string.IsNullOrWhiteSpace(backdropPath)
                ? null
                : "https://image.tmdb.org/t/p/original" + backdropPath;
        }

        public static string GetProfileUrl(string profilePath)
        {
            return string.IsNullOrWhiteSpace(profilePath)
                ? "https://via.placeholder.com/120x120?text=?"
                : ImageBaseUrl + profilePath;
        }

        public Task<TmdbPagedResponse<TmdbMovieResult>> GetPopularMoviesAsync()
        {
            return GetAsync<TmdbPagedResponse<TmdbMovieResult>>("movie/popular?language=es-ES&page=1");
        }

        public Task<TmdbPagedResponse<TmdbMovieResult>> GetTrendingMoviesAsync()
        {
            return GetAsync<TmdbPagedResponse<TmdbMovieResult>>("trending/movie/day?language=es-ES");
        }

        public Task<TmdbPagedResponse<TmdbMovieResult>> SearchMoviesAsync(string query)
        {
            var encodedQuery = HttpUtility.UrlEncode(query);
            return GetAsync<TmdbPagedResponse<TmdbMovieResult>>("search/movie?language=es-ES&query=" + encodedQuery);
        }

        public Task<TmdbMovieDetail> GetMovieDetailAsync(int id)
        {
            return GetAsync<TmdbMovieDetail>("movie/" + id + "?language=es-ES&append_to_response=videos,credits");
        }

        public Task<TmdbPagedResponse<TmdbTvResult>> GetPopularSeriesAsync()
        {
            return GetAsync<TmdbPagedResponse<TmdbTvResult>>("tv/popular?language=es-ES&page=1");
        }

        public Task<TmdbPagedResponse<TmdbTvResult>> GetTrendingSeriesAsync()
        {
            return GetAsync<TmdbPagedResponse<TmdbTvResult>>("trending/tv/day?language=es-ES");
        }

        public Task<TmdbPagedResponse<TmdbTvResult>> SearchSeriesAsync(string query)
        {
            var encodedQuery = HttpUtility.UrlEncode(query);
            return GetAsync<TmdbPagedResponse<TmdbTvResult>>("search/tv?language=es-ES&query=" + encodedQuery);
        }

        public Task<TmdbTvDetail> GetSeriesDetailAsync(int id)
        {
            return GetAsync<TmdbTvDetail>("tv/" + id + "?language=es-ES&append_to_response=videos,credits");
        }

        public static string GetYoutubeTrailerKey(TmdbVideos videos)
        {
            if (videos?.Results == null)
            {
                return null;
            }

            foreach (var video in videos.Results)
            {
                if (video.Site == "YouTube" && video.Type == "Trailer")
                {
                    return video.Key;
                }
            }

            foreach (var video in videos.Results)
            {
                if (video.Site == "YouTube")
                {
                    return video.Key;
                }
            }

            return null;
        }

        private async Task<T> GetAsync<T>(string endpoint)
        {
            var url = BaseUrl + endpoint + (endpoint.Contains("?") ? "&" : "?") + "api_key=" + _apiKey;
            var response = await Client.GetAsync(url).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
