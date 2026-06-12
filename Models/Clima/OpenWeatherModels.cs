using System.Collections.Generic;
using Newtonsoft.Json;

namespace RedFlix.Models.Clima
{
    public class OpenWeatherResponse
    {
        [JsonProperty("name")]
        public string CityName { get; set; }

        [JsonProperty("weather")]
        public List<OpenWeatherCondition> Weather { get; set; }

        [JsonProperty("main")]
        public OpenWeatherMain Main { get; set; }
    }

    public class OpenWeatherCondition
    {
        [JsonProperty("main")]
        public string Main { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }
    }

    public class OpenWeatherMain
    {
        [JsonProperty("temp")]
        public double Temp { get; set; }
    }

    public class ClimaRecomendacionViewModel
    {
        public string Ciudad { get; set; }
        public string Condicion { get; set; }
        public string Descripcion { get; set; }
        public string Icono { get; set; }
        public double Temperatura { get; set; }
        public int GeneroTmdbId { get; set; }
        public string GeneroNombre { get; set; }
        public string Motivo { get; set; }
    }
}
