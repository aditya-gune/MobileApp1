using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Text.Method;
using Android.Widget;
using Android.OS;

using Android.Util;
using Android.Locations;

using Newtonsoft;
using Newtonsoft.Json;
using Couchbase.Lite;

namespace MobileApp
{
    [Activity(Label = "MyStop")]
    public class MyStop : Activity
    {
        ListView savedStop;
        TextView stat;
        List<string> stopNList = new List<string>();
        Database db;
        const string DB_NAME = "couchbaseevents";
        static string stopid;
        static string stopname;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.MyStop);
            // Create your application here
            stat = FindViewById<TextView>(Resource.Id.getstatus);
            db = Manager.SharedInstance.GetDatabase(DB_NAME);

            string entity = MainActivity.currentUser + "_stops";
            var userstops = db.GetExistingDocument(entity);
            if (userstops != null)
            {
                var stopvar = userstops.GetProperty("homeid");
                if (stopvar != null && stopvar.ToString() != "")
                {
                    stopid = stopvar.ToString();
                    wrapperMethod(stopid);
                    return;
                }
                else
                {
                    string nostop = "No stop saved as Home";
                    stopNList.Add(nostop);
                    populateStopList();
                    return;
                }
            }
            else
            {
                string nostop = "No stop saved as Home";
                stopNList.Add(nostop);
                populateStopList();
                return;
            }
            return;
        }
        public void populateStopList()
        {
            savedStop = FindViewById<ListView>(Resource.Id.savedStop);

            ArrayAdapter<string> adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, stopNList);

            savedStop.Adapter = adapter;
            savedStop.ItemClick += savedStop_ItemClick;
        }
        private void savedStop_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            if (stopid != null)
            {
                var findstop = new Intent(this, typeof(StopDetails));
                findstop.PutExtra("stopid", stopid);
                StartActivity(findstop);
            }
            else
            {
                var redirect = new Intent(this, typeof(StopsList));
                StartActivity(redirect);
            }
        }
        private async void wrapperMethod(string stopid)
        {
            await getStopName(stopid);
            stopNList.Add(stopname);
            populateStopList();
        }
        private async Task getStopName(string stopid)
        {
            stat = FindViewById<TextView>(Resource.Id.getstatus);
            string result;
            string url = "http://realtime.mbta.com/developer/api/v2/predictionsbystop?api_key=EQa2HL0Uck-DhtJehlfq-w&stop="
                             + stopid + "&format=xml";

            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri(url));
                request.ContentType = "application/xml";
                request.Method = "GET";
                using (WebResponse response = await request.GetResponseAsync())
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    result = reader.ReadToEnd();
                    Match match = Regex.Match(result, "stop_name=\".*?\"");
                    stopname = match.ToString();
                    stat.Text += stopname;

                    match = Regex.Match(stopname, "\".*?\"");
                    stopname = match.ToString();
                    if (stopname.Contains('"'))
                        stopname = stopname.Replace("\"", "");
                    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: ", e);
            }


        }
    }
}