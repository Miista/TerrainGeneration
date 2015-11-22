using UnityEngine;
using WeatherControl.Core.Data;

namespace WeatherControl.Plugs.Bridges
{
    public class PlayerLocationProvider : GeolocationProvider
    {
        public override Coord GetLocation()
        {
            return new Coord
            {
                Latitude = transform.position.x,
                Longitude = transform.position.z
            };
        }
    }
}