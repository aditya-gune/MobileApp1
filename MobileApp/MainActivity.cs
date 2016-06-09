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
using Java.Net;

namespace MobileApp
{
    [Activity(Label = "MobileApp", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        int count = 1;
        string username;
        string password;
        const string DB_NAME = "couchbaseevents";
        const string TAG = "CouchbaseEvents";
        Database db;
        public static string currentUser;

        protected override void OnCreate(Bundle bundle)
        {
            
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            
            
            // Get our button from the layout resource,
            // and attach an event to it
            
            HelloCBL();
            createSyncURL(true);


        }
        /*protected override void OnResume()
        {
            base.OnResume();
            EditText textbox = FindViewById<EditText>(Resource.Id.editText1);
            if (outState.GetString("userID") == "")
                textbox.Text = string.Format("No user logged in");
        }*/
        void HelloCBL()
        {
            try
            {
                db = Manager.SharedInstance.GetDatabase(DB_NAME);
            }
            catch (Exception e)
            {
                Log.Error(TAG, "Error getting database", e);
                return;
            }

            Button loginButton = FindViewById<Button>(Resource.Id.LoginButton);
            Button createButton = FindViewById<Button>(Resource.Id.CreateButton);

            loginButton.Click += delegate { LoginUser(); };
            createButton.Click += delegate { CreateUser(); };

            // Create the document

        }
        string LoginUser()
        {
            EditText retUser = FindViewById<EditText>(Resource.Id.username);
            EditText retPass = FindViewById<EditText>(Resource.Id.password);
            username = retUser.Text;
            password = retPass.Text;
            
            TextView textbox = FindViewById<TextView>(Resource.Id.textView5);
            var doc = db.GetExistingDocument(username);
            if (doc == null)
            {
                textbox.Text = string.Format("Invalid username/password");
            }
            else
            {
                var curPass = doc.Properties["password"];
                
                if (curPass.ToString() == password)
                {
                    try
                    {

                        currentUser = username;
                        StartActivity(typeof(MobileApp.NearbyRoutes));
                    }
                    catch (Exception e)
                    {
                        Log.Error(TAG, "Error putting properties to Couchbase Lite database", e);
                    }
                }
                else
                    textbox.Text = string.Format("Invalid username/password");
            }

            return username;
        }
        string CreateUser()
        {
            EditText newuser = FindViewById<EditText>(Resource.Id.newUsername);
            EditText newpass = FindViewById<EditText>(Resource.Id.newPassword);
            username = newuser.Text;
            password = newpass.Text;
            TextView textbox = FindViewById<TextView>(Resource.Id.textView6);
            var doc = db.GetDocument(username);
            
            var props = new Dictionary<string, object> {
                { "username", username },
                { "password", password }
            };
            if (doc.GetProperty("password") == null)
            {
                try
                {

                    doc.PutProperties(props);

                    var request = (HttpWebRequest)WebRequest.Create("http://10.0.0.94:4985/couchbaseevents");

                    var postData = "&username=" + username;
                    postData += "&password=" + password;
                    var data = Encoding.ASCII.GetBytes(postData);

                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = data.Length;

                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }

                    var response = (HttpWebResponse)request.GetResponse();

                    var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();


                    var doc2 = db.GetDocument(username);

                    textbox.Text = string.Format(doc2.GetProperty("username") + ", " + doc2.GetProperty("password"));
                    currentUser = username;
                    StartActivity(typeof(MobileApp.NearbyRoutes));
                }
                catch (Exception e)
                {
                    Log.Error(TAG, "Error putting properties to Couchbase Lite database", e);
                }
            }
            else
                textbox.Text = string.Format("That user already exists");
            return username;
        }

        private URL createSyncURL(bool isEncrypted)
        {
            URL syncURL = null;
            String host = "https://10.0.0.94";
            String port = "4985";
            String dbName = "couchbaseevents";
            try
            {
                syncURL = new URL(host + ":" + port + "/" + dbName);
            }
            catch (MalformedURLException me)
            {
                me.PrintStackTrace();
            }
            return syncURL;
        }

    }
}

