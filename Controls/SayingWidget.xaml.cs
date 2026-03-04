using System;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace MAS_GUI.Controls
{
    public partial class SayingWidget : UserControl
    {
        public SayingWidget()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            FetchSayingAsync();
        }

        private void FetchSayingAsync()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                string saying = null;
                try
                {
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://uapis.cn/api/v1/saying");
                    request.Method = "GET";
                    request.UserAgent = "MAS_gui";
                    request.Timeout = 5000;
                    request.ReadWriteTimeout = 5000;
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    using (Stream stream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string json = reader.ReadToEnd();
                        Match match = Regex.Match(json, "\"text\"\\s*:\\s*\"([^\"]*)\"");
                        if (match.Success)
                        {
                            saying = match.Groups[1].Value;
                        }
                    }
                }
                catch
                {
                }

                if (string.IsNullOrEmpty(saying))
                {
                    saying = string.Empty;
                }

                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new System.Windows.Threading.DispatcherOperationCallback(delegate(object o)
                    {
                        UpdateSayingText((string)o);
                        return null;
                    }),
                    saying);
            });
        }

        private void UpdateSayingText(string text)
        {
            SayingText.Text = text;
        }
    }
}
