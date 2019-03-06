using Android;
using Android.App;
using Android.Content.PM;
using Android.Gms.Location;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Android.Views;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MapApp
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, IOnMapReadyCallback
    {
        private GoogleMap _map;
        private FusedLocationProviderClient _fusedLocationProviderClient;
        private MarkerOptions _userMarker;
        private List<MarkerOptions> _mapMarkers;
        private LocationRequest _locationRequest;
        private FusedLocationProviderCallback _locationCallback;
        private bool _isZoomed;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            _locationRequest = new LocationRequest().SetPriority(LocationRequest.PriorityHighAccuracy);
            _locationCallback = new FusedLocationProviderCallback(this);
            _fusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(this);

            var mapFragment = (MapFragment)FragmentManager.FindFragmentById(Resource.Id.map);
            mapFragment.GetMapAsync(this);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;
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
            ZoomMapOnUserLocation();
        }

        private void ZoomMapOnUserLocation()
        {
            if (_userMarker == null || _map == null)
                return;

            var cameraUpdate = CameraUpdateFactory.NewLatLngZoom(_userMarker.Position, 10);
            _map.MoveCamera(cameraUpdate);
            _isZoomed = true;
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            _isZoomed = false;
            _map = googleMap;

            if (_mapMarkers == null)
                _mapMarkers = new List<MarkerOptions>();

            // Add zoom in/out buttons
            _map.UiSettings.ZoomControlsEnabled = true;

            // Add custom marker to the map
            var customMarker = CreateMapMarker(new LatLng(50.0, 50.0), "My custom marker", BitmapDescriptorFactory.HueBlue);
            _mapMarkers.Add(customMarker);

            GetUserLastPosition();

            RefreshMapMarkers();
        }

        private void GetUserLastPosition()
        {
            Func<Task<LatLng>> getLastLocation = async () =>
            {
                var userPosition = await GetLastLocationFromDevice();
                return userPosition;
            };
            Task.Run(getLastLocation).ContinueWith(c =>
            {
                try
                {
                    if (c.Result != null)
                    {
                        UpdateUserLocation(c.Result.Latitude, c.Result.Longitude);
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void RefreshMapMarkers()
        {
            if (_map == null || _mapMarkers == null)
                return;

            _map.Clear();

            foreach (var mapMarker in _mapMarkers)
            {
                _map.AddMarker(mapMarker);
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

        public void UpdateUserLocation(double latitude, double longitude)
        {
            var userPosition = new LatLng(latitude, longitude);

            if (_userMarker == null)
            {
                _userMarker = CreateMapMarker(userPosition, "Ty", BitmapDescriptorFactory.HueRed);
                _mapMarkers.Add(_userMarker);
            }
            else
                _userMarker.SetPosition(userPosition);

            RefreshMapMarkers();

            if (!_isZoomed)
                ZoomMapOnUserLocation();
        }

        private async Task StartRequestingLocationUpdates()
        {
            await _fusedLocationProviderClient.RequestLocationUpdatesAsync(_locationRequest, _locationCallback);
        }

        private async void StopRequestLocationUpdates()
        {
            await _fusedLocationProviderClient.RemoveLocationUpdatesAsync(_locationCallback);
        }

        protected override async void OnResume()
        {
            base.OnResume();

            if (CheckSelfPermission(Manifest.Permission.AccessFineLocation) == Permission.Granted)
            {
                await StartRequestingLocationUpdates();
            }
            else
            {
                ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.AccessFineLocation }, 1);
            }
        }

        protected override void OnPause()
        {
            StopRequestLocationUpdates();
            base.OnPause();
        }
    }
}

