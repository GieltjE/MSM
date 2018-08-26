// 
// This file is a part of MSM (Multi Server Manager)
// Copyright (C) 2016-2018 Michiel Hazelhof (michiel@hazelhof.nl)
// 
// MSM is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// MSM is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// If not, see <http://www.gnu.org/licenses/>.
// 
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Text;

namespace MSM.Extends
{
    public class WebClientOptimized : WebClient
    {
        private readonly CookieContainer _cookieContainer = new CookieContainer();
        private readonly Int32 _timeout;
        private Boolean _preauth;
        private readonly Boolean _useCookieContainer;
        public Encoding EncodingIfNoTDetected = Encoding.UTF8;
        public Boolean AllowAutoRedirects = true;
        private readonly Boolean _keepAlive;

        public WebClientOptimized(Int32 timeout = 30, Boolean useCookieContainer = true, Boolean keepAlive = true)
        {
            CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
            _timeout = timeout;
            _useCookieContainer = useCookieContainer;
            _keepAlive = keepAlive;
        }

        public new String DownloadString(Uri uri)
        {
            // Perform our own characterset detection, the default one breaks quite a lot
            HttpWebRequest req = (HttpWebRequest)GetWebRequest(uri);
            if (req == null)
            {
                return "";
            }

            req.SendChunked = false;
            req.ContentLength = 0;

            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
            {
                // Both UTF-8 and utf-8 are received from time to time
                if (String.Equals(resp.CharacterSet, "UTF-8", StringComparison.OrdinalIgnoreCase))
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    using (StreamReader streamReader = new StreamReader(resp.GetResponseStream(), Encoding.UTF8, true))
                    {
                        // Trim ending newlines, some scripts rely on this! (cd.php etc)
                        return streamReader.ReadToEnd().TrimEnd('\n').TrimEnd('\r');
                    }
                }

                // Works for at least resp.CharacterSet == ISO-8859-1
                if (String.Equals(resp.CharacterSet, "ISO-8859-1", StringComparison.OrdinalIgnoreCase))
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    using (StreamReader streamReader = new StreamReader(resp.GetResponseStream(), Encoding.Default, true))
                    {
                        return streamReader.ReadToEnd().TrimEnd('\n').TrimEnd('\r');
                    }
                }

                // ReSharper disable once AssignNullToNotNullAttribute
                using (StreamReader streamReader = new StreamReader(resp.GetResponseStream(), EncodingIfNoTDetected, true))
                {
                    return streamReader.ReadToEnd().TrimEnd('\n').TrimEnd('\r');
                }
            }
        }
        public new String DownloadString(String uri)
        {
            return DownloadString(new Uri(uri));
        }

        protected override void OnDownloadDataCompleted(DownloadDataCompletedEventArgs e)
        {
            base.OnDownloadDataCompleted(e);
            SetHeaders();
        }
        protected override void OnDownloadStringCompleted(DownloadStringCompletedEventArgs e)
        {
            base.OnDownloadStringCompleted(e);
            SetHeaders();
        }
        protected override void OnDownloadFileCompleted(AsyncCompletedEventArgs e)
        {
            base.OnDownloadFileCompleted(e);
            SetHeaders();
        }
        protected override void OnUploadDataCompleted(UploadDataCompletedEventArgs e)
        {
            base.OnUploadDataCompleted(e);
            SetHeaders();
        }
        protected override void OnUploadStringCompleted(UploadStringCompletedEventArgs e)
        {
            base.OnUploadStringCompleted(e);
            SetHeaders();
        }
        protected override void OnUploadValuesCompleted(UploadValuesCompletedEventArgs e)
        {
            base.OnUploadValuesCompleted(e);
            SetHeaders();
        }
        protected override void OnUploadFileCompleted(UploadFileCompletedEventArgs e)
        {
            base.OnUploadFileCompleted(e);
            SetHeaders();
        }
        protected override void OnOpenReadCompleted(OpenReadCompletedEventArgs e)
        {
            base.OnOpenReadCompleted(e);
            SetHeaders();
        }
        protected override void OnOpenWriteCompleted(OpenWriteCompletedEventArgs e)
        {
            base.OnOpenWriteCompleted(e);
            SetHeaders();
        }

        private void SetHeaders()
        {
            SetHeader(HttpRequestHeader.UserAgent, "MSM");
            SetHeader(HttpRequestHeader.CacheControl, "no-cache");
            SetHeader(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
        }
        private void SetHeader(HttpRequestHeader header, String value)
        {
            try
            {
                Headers[header] = value;
            }
            catch (Exception)
            {
                try
                {
                    Headers.Set(header, value);
                }
                catch {}
            }
        }
        public void AddCredentials(String username, String password)
        {
            // Just perform a basic authentication for other sites
            Headers.Set(HttpRequestHeader.Authorization, "Basic " + Convert.ToBase64String(new UTF8Encoding().GetBytes((username + ":" + password).ToCharArray())));
            Credentials = new NetworkCredential(username, password);
            _preauth = true;
        }
        public void RemoveCredentials()
        {
            Headers.Remove(HttpRequestHeader.Authorization);
            Credentials = null;
            _preauth = false;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            SetHeaders();
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);

            if (request == null) return null;

            if (_useCookieContainer)
            {
                request.CookieContainer = _cookieContainer;
            }

            request.KeepAlive = _keepAlive;
            request.PreAuthenticate = _preauth;
            request.Timeout = _timeout * 1000;
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.AllowAutoRedirect = AllowAutoRedirects;
            return request;
        }
    }
}