using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RestSharp;
using UnityEngine;
using WeatherControl.Core.Data;
using WeatherControl.Plugs;

namespace WeatherControl.Core
{
    public class WeatherController : MonoBehaviour
    {
        public GeolocationProvider LocationProvider;
        public WeatherSystem WeatherSystem;

        //    private readonly string Base = "http://api.openweathermap.org/data/2.5/weather?lat=0&lon=0&APPID=7472543b52ecdbc0d9c848fd2a3364ed&units=metric

        private DateTime _lastFetch = DateTime.Now;
        private readonly RestClient _client;

        private bool ShouldFetch
        {
            get { return (DateTime.Now - _lastFetch).Seconds > 5; }
        }

        public WeatherController()
        {
            _client = new RestClient("http://api.openweathermap.org/data/2.5/");
            _client.AddDefaultParameter("APPID", "7472543b52ecdbc0d9c848fd2a3364ed", ParameterType.GetOrPost);
        }

        // Use this for initialization
        private void Start()
        {
//            LocationProvider = LocationProvider ?? new DefaultGeolocationProvider();
            FetchWeatherData();
        }

        // Update is called once per frame
        private void Update()
        {
            if ( ShouldFetch )
            {
                FetchWeatherData();
            }
        }

        private void FetchWeatherData()
        {
            _lastFetch = DateTime.Now;
            var location = LocationProvider.GetLocation();
            var request = _client.CreateRequest("weather", Method.GET);
            request.AddParameter("lat", location.Latitude)
                   .AddParameter("lon", location.Longitude);
            request.RequestFormat = DataFormat.Json;
            request.JsonSerializer = new RestSharpJsonNetSerializer();
            var response = _client.Get<RootObject>(request);
            print( response.Content );
            WeatherSystem.StartRain();
        }
    }

    public static class RestClientExtensions
    {
        public static IRestRequest CreateRequest(this IRestClient self, string resource, Method method)
        {
            RestRequest request = new RestRequest(resource, method);
            foreach (Parameter param in self.DefaultParameters)
            {
                request.AddParameter(param);
            }
            return request;
        }
    }

}

namespace WeatherControl.Core.Data
{
    public class Coord
    {
        [JsonProperty("lon")]
        public double Longitude { get; set; }
        [JsonProperty("lat")]
        public double Latitude { get; set; }
    }

    public class Weather
    {
        /// <summary>
        ///     Weather condition id. See http://openweathermap.org/weather-conditions
        /// </summary>
        public int id { get; set; }
        public string main { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
    }

    public class Main
    {
        public double temp { get; set; }
        public double pressure { get; set; }
        public int humidity { get; set; }
        public double temp_min { get; set; }
        public double temp_max { get; set; }

        /// <summary>
        ///     Atmospheric pressure on the sea level in hPa
        /// </summary>
        public double sea_level { get; set; }

        /// <summary>
        ///     Atmospheric pressure on the ground level in hPa
        /// </summary>
        public double grnd_level { get; set; }
    }

    public class Wind
    {
        /// <summary>
        ///     Wind speed in meter/sec.
        /// </summary>
        public double speed { get; set; }

        /// <summary>
        ///     Wind direction in meteorological degrees.
        /// </summary>
        public double deg { get; set; }
    }

    public class Clouds
    {
        public int all { get; set; }
    }

    public class Sys
    {
        public string country { get; set; }
        public int sunrise { get; set; }
        public int sunset { get; set; }
    }

    public class RootObject
    {
        public Coord coord { get; set; }

        [JsonProperty("weather")]
        public List<Weather> WeatherConditions { get; set; }
        public Main main { get; set; }
        public Wind wind { get; set; }
        public Clouds clouds { get; set; }
        public int dt { get; set; }
        public Sys sys { get; set; }
        public int id { get; set; }
        public string name { get; set; }
    }

    public class WeatherData
    {
        public WeatherCondition Conditions { get; set; }
        public WindCondition Wind { get; set; }
        public AtmosphereCondition Atmosphere { get; set; }
        public RainCondition Rain { get; set; }
    }

    public enum SnowCondition
    {
        LightSnow,
        Snow,
        HeavySnow,
        Sleet,
        ShowerSleet,
        LightRainAndSnow,
        RainAndSnow,
        LightShowerSnow,
        ShowerSnow,
        HeavyShowerSnow
    }

    public enum RainCondition
    {
        LightRain,
        ModerateRain,
        HeavyIntensityRain,
        VeryHeavyRain,
        ExtremeRain,
        FreezingRain,
        LightIntensityShowerRain,
        ShowerRain,
        HeavyIntensityShowerRain,
        RaggedShowerRain
    }

    public enum AtmosphereCondition
    {
        Mist,
        Smoke,
        Haze,
        Sand,
        Fog,
        Dust,
        VolcanicAsh,
        Squalls,
        Tornado
    }

    public enum WindCondition
    {
        Calm,
        LightBreeze,
        GentleBreeze,
        ModerateBreeze,
        FreshBreeze,
        StrongBreeze,
        HighWind,
        Gale,
        SevereGale,
        Storm,
        ViolentStorm,
        Hurricane
    }

    public enum WeatherCondition
    {
        Snow, Rain, Thunder, Clear
    }
}

namespace WeatherControl.Plugs
{
    public abstract class GeolocationProvider : MonoBehaviour
    {
        public abstract Coord GetLocation();
    }

    public abstract class WeatherSystem : MonoBehaviour
    {
        public abstract void StartRain();

        public abstract void SetCloudCover(int cloudiness);
    }
}

namespace WeatherControl.Plugs.Bridges
{
    internal class DefaultGeolocationProvider : GeolocationProvider
    {
        public override Coord GetLocation()
        {
            return new Coord
            {
                Latitude = 0.0,
                Longitude = 0.0
            };
        }
    }
}