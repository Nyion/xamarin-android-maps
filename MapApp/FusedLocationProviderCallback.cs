using Android.Gms.Location;
using System.Linq;

namespace MapApp
{
    public class FusedLocationProviderCallback : LocationCallback
    {
        readonly MainActivity activity;

        public FusedLocationProviderCallback(MainActivity activity)
        {
            this.activity = activity;
        }

        public override void OnLocationResult(LocationResult result)
        {
            if (result.Locations.Any())
            {
                var location = result.Locations.First();
                activity.UpdateUserLocation(location.Latitude, location.Longitude);
            }
        }
    }
}