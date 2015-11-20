extern alias lib;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WS = lib::WebSurge;

namespace JayKay.WebSurge.Plugin
{

    public class PropagateCookiePlugIn : WS.IWebSurgeExtensibility
    {
        private Dictionary<int, Dictionary<string, string>> CookiesByThread;

        public PropagateCookiePlugIn()
        { 
            CookiesByThread = new Dictionary<int, Dictionary<string, string>>();
        }

        public void OnAfterRequestSent(WS.HttpRequestData data)
        {
            if (!CookiesByThread.ContainsKey(Thread.CurrentThread.ManagedThreadId))
            {
                CookiesByThread.Add(Thread.CurrentThread.ManagedThreadId, new Dictionary<string, string>());
            }

            var cookies = CookiesByThread[Thread.CurrentThread.ManagedThreadId];

            string cookie = data.GetResponseHeader("Set-Cookie");

            if (!string.IsNullOrEmpty(cookie))
            {
                Uri uri = new Uri(data.Url);

                if (cookies.ContainsKey(uri.Host))
                {
                    cookies[uri.Host] = cookie;
                }
                else
                {
                    cookies.Add(uri.Host, cookie);
                }
            }
        }

        public bool OnBeforeRequestSent(WS.HttpRequestData data)
        {
            if (!CookiesByThread.ContainsKey(Thread.CurrentThread.ManagedThreadId))
            {
                CookiesByThread.Add(Thread.CurrentThread.ManagedThreadId, new Dictionary<string, string>());
            }

            var cookies = CookiesByThread[Thread.CurrentThread.ManagedThreadId];

            Uri uri = new Uri(data.Url);
            if (cookies.ContainsKey(uri.Host))
            {
                var cookie = data.Headers?.FirstOrDefault(h => h.Name == "Cookie");
                if (cookie != null)
                {
                    cookie.Value = cookies[uri.Host];
                }
                else
                {
                    data.Headers.Add(new WS.HttpRequestHeader() { Name = "Cookie", Value = cookies[uri.Host] });
                }
            }
            else
            {
                var cookie = data.Headers?.FirstOrDefault(h => h.Name == "Cookie");
                if (cookie != null)
                {
                    data.Headers.Remove(cookie);
                }
            }

            return true;
        }

        public void OnLoadTestCompleted(IList<WS.HttpRequestData> results, int timeTakenForTestMs)
        {
        }

        public bool OnLoadTestStarted(IList<WS.HttpRequestData> requests)
        {
            return true;
        }
    }
}
