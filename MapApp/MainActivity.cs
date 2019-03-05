using Android.App;
using Android.Gms.Location;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using System;
using System.Threading.Tasks;

namespace MapApp
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, IOnMapReadyCallback
    {
        private GoogleMap _map;
        private FusedLocationProviderClient _fusedLocationProviderClient;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            _fusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(this);

            var mapFragment = (MapFragment)FragmentManager.FindFragmentById(Resource.Id.map);
            mapFragment.GetMapAsync(this);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            //FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            //fab.Click += FabOnClick;
        }



        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View)sender;
            Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
                .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            _map = googleMap;

            // Add zoom in/out buttons
            _map.UiSettings.ZoomControlsEnabled = true;

            // Add custom marker to the map
            var customMarker = CreateMapMarker(new LatLng(50.0, 50.0), "My custom marker", BitmapDescriptorFactory.HueBlue);
            _map.AddMarker(customMarker);

            var userPosition = GetLastLocationFromDevice().Result;
            if (userPosition != null)
            {
                var userMarker = CreateMapMarker(userPosition, "User", BitmapDescriptorFactory.HueRed);
                _map.AddMarker(userMarker);
            }
        }

        private MarkerOptions CreateMapMarker(LatLng location, string title, float markerColor)
        {
            MarkerOptions markerOpt1 = new MarkerOptions();
            markerOpt1.SetPosition(location);
            markerOpt1.SetTitle(title);

            var bmDescriptor = BitmapDescriptorFactory.DefaultMarker(markerColor);
            markerOpt1.SetIcon(bmDescriptor);
            return markerOpt1;
        }

        private async Task<LatLng> GetLastLocationFromDevice()
        {
            Android.Locations.Location location = await _fusedLocationProviderClient.GetLastLocationAsync();

            if (location == null)
                return null;

            return new LatLng(location.Latitude, location.Longitude);
        }
    }
}

