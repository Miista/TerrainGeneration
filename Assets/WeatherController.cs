using System;
using System.Collections.Generic;
using Data;
using Newtonsoft.Json;
using RestSharp;
using UnityEngine;
using WeatherControl;

public class WeatherController : MonoBehaviour
{
    public GeolocationProvider LocationProvider;
    public WeatherSystem WeatherSystem;
    public double Latitude, Longitude;

//    private readonly string Base = "http://api.openweathermap.org/data/2.5/weather?lat=0&lon=0&APPID=7472543b52ecdbc0d9c848fd2a3364ed&units=metric

    private readonly IDictionary<string, string> _defaultParameters = new Dictionary<string, string>
    {
        { "APPID", "7472543b52ecdbc0d9c848fd2a3364ed" }
    };

    private DateTime _lastFetch = DateTime.Now;
    private readonly RestClient _client;

    private bool ShouldFetch
    {
        get { return ( DateTime.Now - _lastFetch ).Seconds > 5; }
    }

    public WeatherController()
    {
        _client = new RestClient("http://api.openweathermap.org/data/2.5/");
        _client.AddDefaultParameter("APPID", "7472543b52ecdbc0d9c848fd2a3364ed", ParameterType.GetOrPost);
    }

	// Use this for initialization
	void Start ()
	{
	    LocationProvider = LocationProvider ?? new DefaultGeolocationProvider();
        FetchWeatherData();
	}

    // Update is called once per frame
	void Update ()
	{
	    if ( !ShouldFetch )
	    {
	        return;
	    }
	    FetchWeatherData();
	}

    private void FetchWeatherData()
    {
        _lastFetch = DateTime.Now;
        var request = _client.CreateRequest( "weather", Method.GET );
        request.AddParameter( "lat", Latitude )
               .AddParameter( "lon", Longitude );
        var response = _client.Get<object>( request );
        RootObject deserializeObject = JsonConvert.DeserializeObject<RootObject>( response.Content );
        WeatherSystem.StartRain();
    }
}

public static class RestClientExtensions
{
    public static IRestRequest CreateRequest(this IRestClient self, string resource, Method method)
    {
        RestRequest request = new RestRequest( resource, method );
        foreach (Parameter param in self.DefaultParameters)
        {
            request.AddParameter( param );
        }
        return request;
    }
}

namespace Data
{
    public class Coord
    {
        public double lon { get; set; }
        public double lat { get; set; }
    }

    public class Weather
    {
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
        public double sea_level { get; set; }
        public double grnd_level { get; set; }
    }

    public class Wind
    {
        public double speed { get; set; }
        public double deg { get; set; }
    }

    public class Clouds
    {
        public int all { get; set; }
    }

    public class Sys
    {
        public double message { get; set; }
        public string country { get; set; }
        public int sunrise { get; set; }
        public int sunset { get; set; }
    }

    public class RootObject
    {
        public Coord coord { get; set; }
        public List<Weather> weather { get; set; }
        public string @base { get; set; }
        public Main main { get; set; }
        public Wind wind { get; set; }
        public Clouds clouds { get; set; }
        public int dt { get; set; }
        public Sys sys { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public int cod { get; set; }
    }

}

namespace WeatherControl
{
    public abstract class GeolocationProvider : UnityEngine.Object
    {
        public abstract Geolocation GetLocation();
    }

    internal class DefaultGeolocationProvider : GeolocationProvider
    {
        public override Geolocation GetLocation()
        {
            return new Geolocation
            {
                Latitude = 0.0,
                Longitude = 0.0
            };
        }
    }

    public class Geolocation
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public abstract class WeatherSystem : MonoBehaviour, IWeatherSystem
    {
        public abstract void UpdateWeather(string t);

        public abstract void StartRain();
    }

    public interface IWeatherSystem
    {
        void UpdateWeather(string t);
        void StartRain();
    }
}