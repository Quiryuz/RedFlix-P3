using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using RedFlix.Models.Clima;

namespace RedFlix.Services
{
    public class ClimaService
    {
        private const string BaseUrl = "https://api.openweathermap.org/data/2.5/weather";
        private static readonly HttpClient Cliente = new HttpClient();

        private readonly string _apiKey;
        private readonly string _ciudad;

        public ClimaService()
        {
            _apiKey = ConfigurationManager.AppSettings["OpenWeatherApiKey"];
            _ciudad = ConfigurationManager.AppSettings["OpenWeatherCity"] ?? "Montevideo,UY";

            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                throw new ConfigurationErrorsException("Falta la clave OpenWeatherApiKey en Web.config.");
            }
        }

        public async Task<ClimaRecomendacionViewModel> ObtenerRecomendacionPorClimaAsync()
        {
            var url = BaseUrl
                + "?q=" + HttpUtility.UrlEncode(_ciudad)
                + "&appid=" + _apiKey
                + "&units=metric&lang=es";

            var respuesta = await Cliente.GetAsync(url).ConfigureAwait(false);
            respuesta.EnsureSuccessStatusCode();

            var json = await respuesta.Content.ReadAsStringAsync().ConfigureAwait(false);
            var clima = JsonConvert.DeserializeObject<OpenWeatherResponse>(json);

            return ConstruirRecomendacion(clima);
        }

        private static ClimaRecomendacionViewModel ConstruirRecomendacion(OpenWeatherResponse clima)
        {
            var condicion = clima?.Weather?.FirstOrDefault();
            var main = condicion?.Main ?? "Clear";
            var descripcion = condicion?.Description ?? "cielo despejado";
            var temperatura = clima?.Main?.Temp ?? 0;

            var generoId = 35;
            var generoNombre = "Comedia";
            var motivo = "El clima esta tranquilo, ideal para peliculas alegres y livianas.";

            switch (main)
            {
                case "Thunderstorm":
                    generoId = 27;
                    generoNombre = "Terror";
                    motivo = "Hay tormenta, buen momento para una pelicula intensa de terror.";
                    break;
                case "Drizzle":
                case "Rain":
                    generoId = 18;
                    generoNombre = "Drama";
                    motivo = "La lluvia acompana historias emotivas y dramaticas.";
                    break;
                case "Snow":
                    generoId = 10751;
                    generoNombre = "Familia";
                    motivo = "Con frio o nieve, una pelicula familiar funciona perfecto.";
                    break;
                case "Mist":
                case "Smoke":
                case "Haze":
                case "Dust":
                case "Fog":
                case "Sand":
                case "Ash":
                case "Squall":
                case "Tornado":
                    generoId = 53;
                    generoNombre = "Thriller";
                    motivo = "El clima neblinoso combina con suspenso y misterio.";
                    break;
                case "Clouds":
                    generoId = 9648;
                    generoNombre = "Misterio";
                    motivo = "Un dia nublado va bien con historias de misterio.";
                    break;
                case "Clear":
                    generoId = 35;
                    generoNombre = "Comedia";
                    motivo = "Si esta soleado, recomendamos peliculas alegres.";
                    break;
            }

            if (temperatura <= 10 && main != "Thunderstorm")
            {
                generoId = 27;
                generoNombre = "Terror";
                motivo = "Con baja temperatura, una pelicula de terror suma ambiente.";
            }

            return new ClimaRecomendacionViewModel
            {
                Ciudad = clima?.CityName ?? "Montevideo",
                Condicion = main,
                Descripcion = descripcion,
                Icono = condicion?.Icon,
                Temperatura = temperatura,
                GeneroTmdbId = generoId,
                GeneroNombre = generoNombre,
                Motivo = motivo
            };
        }
    }
}
