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
using System.Runtime.Serialization.Formatters.Binary;

namespace MobileApp
{
    [Activity(Label = "StopDetails")]
    public class StopDetails : Activity
    {
        public static string stopid;
        public static string stopname;
        TextView stopDetails;
        TextView stopInfo;
        List<string> routes;
        List<string> sch_arrs;
        string detailString;
        Button savehome;
        Button saveWork;
        Database db;
        const string DB_NAME = "couchbaseevents";
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.StopDetails);
            stopid = Intent.GetStringExtra("stopid") ?? "Data not available";

            db = Manager.SharedInstance.GetDatabase(DB_NAME);
            createDb();

            stopDetails = FindViewById<TextView>(Resource.Id.stopDetails);

            
            //stopInfo.MovementMethod = new Android.Text.();
            stopDetails.Text = stopid;
            populateStopDetails(stopid);

            // Create your application here
        }
        private void createDb()
        {
            string entity = MainActivity.currentUser + "_stops";
            var userstops = db.GetExistingDocument(entity);
            if (userstops == null)
            {
                try
                {
                    var stops = new Dictionary<string, object> {
                                { "homeid", null },
                                { "homename", null },
                                { "workid", null },
                                { "workname", null }
                            };
                    userstops.PutProperties(stops);
                }
                catch (Exception ex)
                {
                    Log.Error("error:", "Error putting properties to Couchbase Lite database", ex);
                }

            }
            else
            {
                savehome = FindViewById<Button>(Resource.Id.saveHome);
                var curStop = userstops.Properties["homeid"];
                if (curStop.ToString() == stopid)
                    savehome.Text = "Unsave";
            }
        }
        private void saveHomeClick(object sender, EventArgs e)
        {
            string entity = MainActivity.currentUser + "_stops";
            var userstops = db.GetDocument(entity);
            if (userstops.GetProperty("homeid") == null)
            {
                userstops.Update((UnsavedRevision newRevision) =>
                {
                    var properties = newRevision.Properties;
                    properties["homeid"] = stopid;
                    properties["homename"] = stopname;
                    return true;
                });
                string url = "http://10.0.0.94:4985/couchbaseevents";
                WebRequest request = WebRequest.Create(url);
                request.Method = "DELETE";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                savehome.Text = "Unsave";

            }
            else if (userstops.GetProperty("homeid").ToString() == stopid)
            {
                savehome = FindViewById<Button>(Resource.Id.saveHome);
                userstops.Update((UnsavedRevision newRevision) =>
                {
                    var properties = newRevision.Properties;
                    properties["homeid"] = "";
                    properties["homename"] = "";
                    return true;
                });
                string url = "http://10.0.0.94:4985/couchbaseevents";
                byte[] nullbyte;
                var data = new Dictionary < string, object> {
                        { "username", null },
                        { "password", null }
                };
                var binFormatter = new BinaryFormatter();
                var mStream = new MemoryStream();
                binFormatter.Serialize(mStream, data);

                using (var client = new System.Net.WebClient())
                {
                    client.UploadData(url, "PUT", mStream.ToArray());
                }
                savehome.Text = "Save as Home";
            }
            else
            {
                userstops.Update((UnsavedRevision newRevision) =>
                {
                    var properties = newRevision.Properties;
                    properties["homeid"] = stopid;
                    properties["homename"] = stopname;
                    return true;
                });
                savehome.Text = "Unsave";
            }

        }
        private async void populateStopDetails(string stopid)
        {
            await getStopDetails(stopid);
            savehome = FindViewById<Button>(Resource.Id.saveHome);
            savehome.Click += saveHomeClick;

            string entity = MainActivity.currentUser + "_stops";
            var userstops = db.GetDocument(entity);

            

        }
        private async Task getStopDetails(string stopid)
        {
            string result;
            stopDetails = FindViewById<TextView>(Resource.Id.stopDetails);
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
                    Console.WriteLine(result);
                    parseStopString(result);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: ", e);
            }


        }
        private void parseStopString(string result)
        {
            stopInfo = FindViewById<TextView>(Resource.Id.stopInfo);

            //Get stop name
            Match match = Regex.Match(result, "stop_name=\".*?\"");
            string stopname = match.ToString();
            match = Regex.Match(stopname, "\".*?\"");
            stopname = match.ToString();
            if (stopname.Contains('"'))
                stopname = stopname.Replace("\"", "");

            stopDetails.Text = stopname;


            string routePattern = "route_name=\".*?\"";
            string dirPattern = "direction_name=\".*?\"";
            string pre_away_pattern = "pre_away=\".*?\"";
            string sch_arr_pattern = "sch_arr_dt=\".*?\"";
            string pre_away;
            string sch_arr;
            string routename;
            string tripinfo;
            string dirname;
            double tDouble;
            int temp;
            int i = 0;

            //Get the trip route
            foreach (Match routematch in Regex.Matches(result, routePattern))
            {
                Console.WriteLine("check route...\n");
                match = Regex.Match(routematch.ToString(), "\".*?\"");
                routename = match.ToString();
                if (routename.Contains('"'))
                    routename = routename.Replace("\"", "");
                
                //Get the trip direction
                foreach(Match dirMatch in Regex.Matches(result, dirPattern))
                {
                    pre_away = "";
                    Console.WriteLine("check direction...");
                    match = Regex.Match(dirMatch.ToString(), "\".*?\"");
                    dirname = match.ToString();
                    if (dirname.Contains('"'))
                        dirname = dirname.Replace("\"", "");
                    stopInfo.Text += "\nRoute " + routename + " - " + dirname + "\n";

                   
                    
                    Console.WriteLine("about to check sch_arr...\n");

                    foreach (Match tripMatch in Regex.Matches(result, pre_away_pattern))
                    {
                        
                            Console.WriteLine("checking pre_away...\n");
                            match = Regex.Match(tripMatch.ToString(), "\".*?\"");
                            pre_away = match.ToString();
                            if (pre_away.Contains('"'))
                                pre_away = pre_away.Replace("\"", "");
                            temp = Convert.ToInt32(pre_away);
                            temp = temp / 60;
                            pre_away = temp.ToString();
                            
                    }

                    
                    stopInfo.Text += "Vehicle Arrival Times: ";

                    //Get the arrival time
                    foreach (Match tripMatch in Regex.Matches(result, sch_arr_pattern))
                    {
                        Console.WriteLine("checking sch_arr...\n");
                        match = Regex.Match(tripMatch.ToString(), "\".*?\"");
                        sch_arr = match.ToString();
                        if (sch_arr.Contains('"'))
                            sch_arr = sch_arr.Replace("\"", "");
                        tDouble = Convert.ToDouble(sch_arr);
                        Console.WriteLine("tdouble = " + tDouble);
                           
                        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                        var arrTime = epoch.AddSeconds(tDouble);

                        sch_arr = arrTime.ToShortTimeString();
                        stopInfo.Text += sch_arr + "\n";
                    }
                                          
                }

            }
        }
    }
}