using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace DynDNSUpdater
{
    class BackgroundUpdater
    {
        private bool _continue;
        private Thread worker;

        public string ServiceUrl;
        public string Domain;
        public string Username;
        public string Password;
        public int UpdateInterval;
        public string IPv4Provider;

        public string LastErrorMessage;
        public string logMessage;
        public EventHandler ErrorCallback;
        public EventHandler SuccessCallback;
        public EventHandler LogCallback;

        public BackgroundUpdater()
        {
            UpdateInterval = 60;
            worker = new Thread(DoWork);
        }

        public void Start()
        {
            _continue = true;
            worker.Start();
        }

        public void Stop()
        {
            _continue = false;
            worker.Abort();
        }

        public bool IsRunning()
        {
            return worker.IsAlive;
        }

        public void DoNow()
        {
            try
            {
                Debug.WriteLine("Updating " + Domain + " with public IP " + getPublicIPAddress());

                string ret = updateIP();

                logMessage = ret;

                //if (ret.StartsWith("!yours") || ret.StartsWith("good") || ret.StartsWith("nochg"))
                LogCallback(this, null);

                Debug.WriteLine("Url return :" + ret);

                SuccessCallback(this, null);

                Debug.WriteLine("OK");

                
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Erreur : " + ex.Message);
                if (ErrorCallback != null)
                {
                    Debug.WriteLine(ex.Message);
                    LastErrorMessage = ex.Message;
                    ErrorCallback(this, null);
                }
            }
        }

        public void DoWork()
        {
            Debug.WriteLine("Thread starting");
            while(_continue) {
                DoNow();
                Thread.Sleep(UpdateInterval * 1000);
            }
            Debug.WriteLine("Thread stopped");
        }

        private string getPublicIPAddress()
        {
            
            string addr = httpGet(Properties.Settings.Default.IPv4Provider);
            return addr.Replace("\n", "");
        }

        private string updateIP()
        {
            string ipAddr = getPublicIPAddress();
            string url = "http://" + ServiceUrl + "/nic/update?system=dyndns&hostname=" + Domain + "&myip=" + getPublicIPAddress();
            return httpGet(url, new NetworkCredential(Username, Password));
        }

        private string httpGet(string url, NetworkCredential credentials = null)
        {
            WebClient request = new WebClient();
            if (credentials != null)
            {
                request.Credentials = credentials;
            }
            return request.DownloadString(url);
        }
    }
}
