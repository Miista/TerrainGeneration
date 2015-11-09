using UnityEngine;

namespace WeatherControl.Bridges
{
    public class RainMakerImpl : WeatherSystem {
        public TextMesh Text;
        public RainScript RainPrefab;

        public override void UpdateWeather(string t)
        {
            Text.text = t;
        }

        public override void StartRain()
        {
            RainPrefab.RainIntensity = 0.5f;
            Text.text = "It's raining, bitch!";
        }
    }
}