using UnityEngine;

namespace WeatherControl.Plugs.Bridges
{
    public class RainMakerImpl : WeatherSystem {
        public TextMesh Text;
        public RainScript RainPrefab;

        public override void StartRain()
        {
            RainPrefab.RainIntensity = 0.5f;
            Text.text = "It's raining, bitch!";
        }

        public override void SetCloudCover(int cloudiness)
        {
            throw new System.NotImplementedException();
        }
    }
}