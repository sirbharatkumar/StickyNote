using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Windows.Threading;

namespace Liquid.Components
{
    /// <summary>
    /// This class is used to make webservice calls to the server
    /// </summary>
    public partial class Comms
    {
        private HttpWebRequest Request { get; set; }
        private object Tag { get; set; }

        private DispatcherTimer _timer = new DispatcherTimer();
        private bool _completed = false;
        private HttpResponseCompleteEventArgs responseEventArgs;
        private bool _useNonUIThreadForCallback = false;

        public Dictionary<string, string> PostValues { get; private set; }

        public event HttpResponseCompleteEventHandler ResponseComplete;
        private void OnResponseComplete(HttpResponseCompleteEventArgs e)
        {
            responseEventArgs = e;
            _completed = true;
        }

        public Comms(Uri requestUri, bool useNonUIThreadForCallback)
        {
            this.Request = (HttpWebRequest)WebRequest.Create(requestUri);
            this.Request.ContentType = "application/x-www-form-urlencoded";
            this.Request.Method = "POST";
            this.PostValues = new Dictionary<string, string>();

            PostValues.Add("Liquid", "License");

            _useNonUIThreadForCallback = useNonUIThreadForCallback;
            SetupTimer();
        }

        public Comms(Uri requestUri, string method, bool useNonUIThreadForCallback, params KeyValuePair<string, string>[] postValues)
        {
            this.Request = (HttpWebRequest)WebRequest.Create(requestUri);
            this.Request.ContentType = "application/x-www-form-urlencoded";
            this.Request.Method = method;
            this.PostValues = new Dictionary<string, string>();
            foreach (var item in postValues)
            {
                this.PostValues.Add(item.Key, item.Value);
            }

            _useNonUIThreadForCallback = useNonUIThreadForCallback;
            SetupTimer();
        }

        public void Execute(object tag)
        {
            Tag = tag;
            Execute();
        }

        public void Execute()
        {
            _completed = false;
            this.Request.BeginGetRequestStream(new AsyncCallback(Comms.BeginRequest), this);
        }

        private void SetupTimer()
        {
            if (!_useNonUIThreadForCallback)
            {
                _timer.Interval = new TimeSpan(0, 0, 0, 0, 15);
                _timer.Tick += new EventHandler(Tick);
                _timer.Start();
            }
        }

        private void Tick(object sender, EventArgs e)
        {
            if (_completed && this.ResponseComplete != null)
            {
                this.ResponseComplete(responseEventArgs);
                _completed = false;
            }
        }

        private static void BeginRequest(IAsyncResult ar)
        {
            Comms helper = ar.AsyncState as Comms;
            if (helper != null)
            {
                using (StreamWriter writer = new StreamWriter(helper.Request.EndGetRequestStream(ar)))
                {
                    foreach (var item in helper.PostValues)
                    {
                        writer.Write("{0}={1}&", item.Key, item.Value);
                    }
                }
                helper.Request.BeginGetResponse(new AsyncCallback(Comms.BeginResponse), helper);
            }
        }

        private static void BeginResponse(IAsyncResult ar)
        {
            Comms helper = ar.AsyncState as Comms;

            if (helper != null)
            {
                HttpWebResponse response = (HttpWebResponse)helper.Request.EndGetResponse(ar);
                if (response != null)
                {
                    Stream stream = response.GetResponseStream();
                    if (stream != null)
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            helper.OnResponseComplete(new HttpResponseCompleteEventArgs(reader.ReadToEnd(), helper.Tag));
                        }
                    }
                }
            }
        }
    }

    public delegate void HttpResponseCompleteEventHandler(HttpResponseCompleteEventArgs e);
    public partial class HttpResponseCompleteEventArgs : EventArgs
    {
        public string Response { get; set; }
        public object Tag { get; set; }

        public HttpResponseCompleteEventArgs(string response, object tag)
        {
            this.Response = response;
            this.Tag = tag;
        }

        public HttpResponseCompleteEventArgs(string response)
        {
            this.Response = response;
        }
    }
}
