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
    [Activity(Label = "StopsList")]
    public class StopsList : Activity
    {
        Database db;
        public ListView listView1;
        const string DB_NAME = "couchbaseevents";
        List<string> stopIds = new List<string>();
        List<string> stopNames = new List<string>();
        Button myStop;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.NearbyStops);

            TextView status = FindViewById<TextView>(Resource.Id.getstatus);
           
            
            db = Manager.SharedInstance.GetDatabase(DB_NAME);

            var locationDoc = db.GetDocument("currentlocation");
            string latitude = locationDoc.Properties["lat"].ToString();
            string longitude = locationDoc.Properties["long"].ToString();

            status.Text = latitude + ", " + longitude;
            populateListView(latitude, longitude);
            myStop = FindViewById<Button>(Resource.Id.MyStops);
            myStop.Click += showMyStop;

        }

        private void showMyStop(object sender, EventArgs e)
        {
            myStop = FindViewById<Button>(Resource.Id.MyStops);
            string entity = MainActivity.currentUser + "_stops";
            var userstops = db.GetDocument(entity);
            if (userstops != null)
            {
                //var id = userstops.Properties["homeid"];
                //if (id != null && id.ToString() != "")
               // {
                    //Console.WriteLine("id = " + id);
                    var savedStopactivity = new Intent(this, typeof(MyStop));
                    StartActivity(savedStopactivity);
                //}
                //else
               // {
                //    myStop.Text = "No Home Stop Saved";
                //}
            }
        }
        private async void populateListView(string latitude, string longitude)
        {
            listView1 = FindViewById<ListView>(Resource.Id.listView1);

            await getRoutes(latitude, longitude);

            ArrayAdapter<string> adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, stopNames);

            listView1.Adapter = adapter;
            listView1.ItemClick += ListView1_ItemClick;
        }

        private void ListView1_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            int index = e.Position;
            TextView status = FindViewById<TextView>(Resource.Id.getstatus);
            var activity2 = new Intent(this, typeof(StopDetails));
            activity2.PutExtra("stopid", stopIds.ElementAt(index));
            StartActivity(activity2);


        }

        private async Task getRoutes(string latitude, string longitude)
       {
           string result;
            TextView status = FindViewById<TextView>(Resource.Id.getstatus);
            int i = 0;

            //overriden members for debugging purposes
            //string lat_override = "42.338567";
            //string long_override = "-71.252364";

            // Create the string.
            string url = "http://realtime.mbta.com/developer/api/v2/stopsbylocation?api_key=EQa2HL0Uck-DhtJehlfq-w&lat="
                           + latitude + "&lon=" + longitude + "&format=xml";

            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri(url));
                request.ContentType = "application/xml";
                request.Method = "GET";
                using (WebResponse response = await request.GetResponseAsync())
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    result = reader.ReadToEnd();
                    //status.Text += result;
                    splitString(result);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: ", e);
            }

       }

        private void splitString(string xmlString)
        {
            TextView status = FindViewById<TextView>(Resource.Id.getstatus);
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
                    //status.Text += stopIds.ElementAt(i) + ": " + stopNames.ElementAt(i) + "\n";
                }
                i++;
            }
        }
    }
}