using System.Collections.Generic;
using Newtonsoft.Json;

namespace RedFlix.Models.Tmdb
{
    public class TmdbPagedResponse<T>
    {
        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("results")]
        public List<T> Results { get; set; }
    }

    public class TmdbMovieResult
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("overview")]
        public string Overview { get; set; }

        [JsonProperty("poster_path")]
        public string PosterPath { get; set; }

        [JsonProperty("backdrop_path")]
        public string BackdropPath { get; set; }

        [JsonProperty("vote_average")]
        public double VoteAverage { get; set; }

        [JsonProperty("release_date")]
        public string ReleaseDate { get; set; }
    }

    public class TmdbTvResult
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("overview")]
        public string Overview { get; set; }

        [JsonProperty("poster_path")]
        public string PosterPath { get; set; }

        [JsonProperty("backdrop_path")]
        public string BackdropPath { get; set; }

        [JsonProperty("vote_average")]
        public double VoteAverage { get; set; }

        [JsonProperty("first_air_date")]
        public string FirstAirDate { get; set; }
    }

    public class TmdbMovieDetail : TmdbMovieResult
    {
        [JsonProperty("runtime")]
        public int? Runtime { get; set; }

        [JsonProperty("genres")]
        public List<TmdbGenre> Genres { get; set; }

        [JsonProperty("credits")]
        public TmdbCredits Credits { get; set; }

        [JsonProperty("videos")]
        public TmdbVideos Videos { get; set; }
    }

    public class TmdbTvDetail : TmdbTvResult
    {
        [JsonProperty("number_of_seasons")]
        public int? NumberOfSeasons { get; set; }

        [JsonProperty("genres")]
        public List<TmdbGenre> Genres { get; set; }

        [JsonProperty("credits")]
        public TmdbCredits Credits { get; set; }

        [JsonProperty("videos")]
        public TmdbVideos Videos { get; set; }
    }

    public class TmdbGenre
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class TmdbCredits
    {
        [JsonProperty("cast")]
        public List<TmdbCastMember> Cast { get; set; }
    }

    public class TmdbCastMember
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("character")]
        public string Character { get; set; }

        [JsonProperty("profile_path")]
        public string ProfilePath { get; set; }
    }

    public class TmdbVideos
    {
        [JsonProperty("results")]
        public List<TmdbVideo> Results { get; set; }
    }

    public class TmdbVideo
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("site")]
        public string Site { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
