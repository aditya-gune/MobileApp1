using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using Android.Util;
using Android.Locations;

using Newtonsoft;
using Newtonsoft.Json;
using Couchbase.Lite;

namespace MobileApp
{
    [Activity(Label = "NearbyRoutes")]
    public class NearbyRoutes : Activity, ILocationListener
    {
        Location _currentLocation;
        LocationManager _locationManager;
        string _locationProvider;
        TextView _locationText;
        int callSuccess;
        Database db;
        const string DB_NAME = "couchbaseevents";
        //ListView _nearbyrouteslist;
        public string[] textarray = { };
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.NearbyRoutes);
            // Create your application here
            _locationText = FindViewById<TextView>(Resource.Id.locationText);
            InitializeLocationManager();
            callSuccess = 0;
            db = Manager.SharedInstance.GetDatabase(DB_NAME);
           // _nearbyrouteslist = FindViewById<ListView>(Resource.Id.nearbyrouteslist);
        }
        public void OnLocationChanged(Location location)
        {

            _currentLocation = location;
            if (_currentLocation == null)
            {
                _locationText.Text = "Unable to determine your location. Try again in a short while.";
            }
            else
            {
                _locationText.Text = string.Format("Current Location: {0:f6}, {1:f6}", _currentLocation.Latitude, _currentLocation.Longitude);
                if (callSuccess < 1)
                {
                    _locationText.Text += "\nTransitioning activity, able to get location = " + callSuccess;
                    var locationDoc = db.GetDocument("currentlocation");

                    
                    if(locationDoc.GetProperty("lat") == null)
                    {
                        try
                        {
                            var coordinates = new Dictionary<string, object> {
                                { "lat", _currentLocation.Latitude.ToString() },
                                { "long", _currentLocation.Longitude.ToString() }
                            };
                            locationDoc.PutProperties(coordinates);
                        }
                        catch (Exception e)
                        {
                            Log.Error("error:", "Error putting properties to Couchbase Lite database", e);
                        }
                    }
                    else
                    {
                        locationDoc.Update((UnsavedRevision newRevision) =>
                        {
                            var properties = newRevision.Properties;
                            properties["lat"] = _currentLocation.Latitude.ToString();
                            properties["long"] = _currentLocation.Longitude.ToString();
                            return true;
                        });
                    }
                    
                    StartActivity(typeof(MobileApp.StopsList));
                    callSuccess++;
                }
                else
                    _locationText.Text += "\nable to get location = " + callSuccess;
                    delay();
                    StartActivity(typeof(MobileApp.StopsList));
            }
        }

        public async void delay()
        {
            await Task.Delay(3000);
        }
        public void OnProviderDisabled(string provider) { }

        public void OnProviderEnabled(string provider) { }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras) { }

        protected override void OnResume()
        {
            base.OnResume();
            _locationManager.RequestLocationUpdates(_locationProvider, 0, 0, this);
        }
        protected override void OnPause()
        {
            base.OnPause();
            _locationManager.RemoveUpdates(this);
        }

        void InitializeLocationManager()
        {
            _locationManager = (LocationManager)GetSystemService(LocationService);
            Criteria criteriaForLocationService = new Criteria
            {
                Accuracy = Accuracy.Fine
            };
            IList<string> acceptableLocationProviders = _locationManager.GetProviders(criteriaForLocationService, true);

            if (acceptableLocationProviders.Any())
            {
                _locationProvider = acceptableLocationProviders.First();
            }
            else
            {
                _locationProvider = string.Empty;
            }
            
        }

        /*private async void getRoutes(Location location)
        {
            string result;
            
            int i = 0;
           
                // Create the string.
                string url = "http://realtime.mbta.com/developer/api/v2/stopsbylocation?api_key=EQa2HL0Uck-DhtJehlfq-w&lat="
                            + location.Latitude + "&lon=" + location.Longitude +"&format=xml";
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri(url));
            request.ContentType = "application/xml";
            request.Method = "GET";
            using (WebResponse response = await request.GetResponseAsync())
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());
                result = reader.ReadToEnd();
                splitString(result);
            }

           try
            {
                var activity2 = new Intent(this, typeof(StopsList));
                Bundle extras = new Bundle();
                extras.PutStringArrayList("stopids", stopIds);
                extras.PutStringArrayList("stopnames", stopNames);
                activity2.PutExtras(extras);
                StartActivity(activity2);
            }
            catch(Exception e)
            {
                Console.WriteLine("error: ", e);
            }
        }

        private void splitString(string xmlString)
        {
            TextView status = FindViewById<TextView>(Resource.Id.statuscheck);
            char[] delimiters = { '>' };
            string[] resultString;
            resultString = xmlString.Split(delimiters);
            resultString = resultString.Skip(1).ToArray();

            

            int i = 0;
            int index;
            string substring;
            
            foreach (string s in resultString)
            {
                if (s.Contains("<stop "))
                {
                    string text = s;
                    index = s.IndexOf('"') + 1;
                    text = text.Remove(0, 15);
                    text = text.Substring(0, 4);
                    if (text.Contains('"'))
                            text = text.Remove(text.IndexOf('"'));
                    stopIds.Add(text);
                    //status.Text += "length: " + text.Length + " string: " + text + "\n";
                       
                        index = s.IndexOf("stop_name=\"") + 11;
                        substring = s.Substring(index);
                        index = substring.IndexOf('"');
                        substring = substring.Remove(index);
                        stopNames.Add(substring);
                       //status.Text += "stopid: " + stopIds.ElementAt(i) + ": " + stopNames.ElementAt(i) + "\n";
                }
                i++;
            }
        }*/
    }

}