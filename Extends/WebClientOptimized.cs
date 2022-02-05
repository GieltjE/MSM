// 
// This file is a part of MSM (Multi Server Manager)
// Copyright (C) 2016-2022 Michiel Hazelhof (michiel@hazelhof.nl)
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
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using MSM.Functions;

namespace MSM.Extends;

public class WebClientOptimized : WebClient
{
    private readonly CookieContainer _cookieContainer = new();
    private readonly Int32 _timeout;
    private readonly Boolean _useCookieContainer;
    public static Encoding EncodingIfNoTDetected = Encoding.UTF8;
    public Boolean AllowAutoRedirects = true;
    public Boolean AllowDownloadAutoRedirects = true;

    public WebClientOptimized(Int32 timeout = 30, Boolean useCookieContainer = true)
    {
        CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
        _timeout = timeout;
        _useCookieContainer = useCookieContainer;
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

        using HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
        // Both UTF-8 and utf-8 are received from time to time
        if (String.Equals(resp.CharacterSet, "UTF-8", StringComparison.OrdinalIgnoreCase))
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            using StreamReader streamReader = new(resp.GetResponseStream(), Encoding.UTF8, true);
            // Trim ending newlines, some scripts rely on this! (cd.php etc)
            return streamReader.ReadToEnd().TrimEnd('\n').TrimEnd('\r');
        }

        // Works for at least resp.CharacterSet == ISO-8859-1
        if (String.Equals(resp.CharacterSet, "ISO-8859-1", StringComparison.OrdinalIgnoreCase))
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            using StreamReader streamReader = new(resp.GetResponseStream(), Encoding.Default, true);
            return streamReader.ReadToEnd().TrimEnd('\n').TrimEnd('\r');
        }

        // ReSharper disable once AssignNullToNotNullAttribute
        using (StreamReader streamReader = new(resp.GetResponseStream(), EncodingIfNoTDetected, true))
        {
            return streamReader.ReadToEnd().TrimEnd('\n').TrimEnd('\r');
        }
    }
    public new String DownloadString(String uri)
    {
        return DownloadString(new Uri(uri));
    }
    public new void DownloadFile(Uri uri, String file)
    {
        // Ensure that the downloaded data is flushed, else if the next bit of code is to fast it doesn't always receive all data.....
        using MemoryStream memoryStream = new();
        WebRequest webRequest = GetWebRequest(uri);

        using (WebResponse webResponse = webRequest.GetResponse())
        {
            if (!AllowDownloadAutoRedirects &&
                (((HttpWebResponse)webResponse).StatusCode == HttpStatusCode.Redirect ||
                 ((HttpWebResponse)webResponse).StatusCode == HttpStatusCode.RedirectKeepVerb ||
                 ((HttpWebResponse)webResponse).StatusCode == HttpStatusCode.RedirectMethod ||
                 ((HttpWebResponse)webResponse).StatusCode == HttpStatusCode.TemporaryRedirect))
            {
                return;
            }

            using Stream webStream = webResponse.GetResponseStream();
            webStream.CopyTo(memoryStream);
        }

        memoryStream.Seek(0, SeekOrigin.Begin);
        FileOperations.CreateFile(file);
        try
        {
            using FileStreamOptimized fileStream = new(file, FileMode.Truncate);
            memoryStream.CopyTo(fileStream);
        }
        catch
        {
            FileOperations.DeleteFile(file);
            throw;
        }
        FileOperations.Unblock(file);
    }
    public new void DownloadFile(String uri, String file)
    {
        DownloadFile(new Uri(uri), file);
    }
    private String _file;
    public new void DownloadFileAsync(Uri address, String fileName)
    {
        _file = fileName;
        base.DownloadFileAsync(address, fileName);
    }
    public new void DownloadFileAsync(Uri address, String fileName, Object userToken)
    {
        _file = fileName;
        base.DownloadFileAsync(address, fileName, userToken);
    }
    public new Byte[] DownloadData(String url)
    {
        try
        {
            return base.DownloadData(url);
        }
        catch (WebException webException)
        {
            if (webException.Response is not HttpWebResponse response || (Int32)response.StatusCode != 308) throw;

            if (webException.Response.Headers.AllKeys.Any(headerKey => String.Equals("Location", headerKey, StringComparison.Ordinal)))
            {
                return DownloadData(webException.Response.Headers["Location"]);
            }
            throw;
        }
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
        if (_file != null)
        {
            FileOperations.Unblock(_file);
        }

        _file = null;
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
        SetHeader(HttpRequestHeader.CacheControl, "no-cache");
        SetHeader(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
        SetHeader(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:86.0) Gecko/20100101 Firefox/86.0");
    }

    private void SetHeader(HttpRequestHeader header, String value)
    {
        try
        {
            Headers[header] = value;
        }
        catch
        {
            try
            {
                Headers.Set(header, value);
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch { }
        }
    }

    protected override WebRequest GetWebRequest(Uri address)
    {
        SetHeaders();
        HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
        if (request == null) return null;

        request.Credentials = Credentials;
        request.KeepAlive = false;

        if (_useCookieContainer)
        {
            request.CookieContainer = _cookieContainer;
        }

        request.Timeout = _timeout * 1000;
        request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
        request.AllowAutoRedirect = AllowAutoRedirects;
        request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:86.0) Gecko/20100101 Firefox/86.0";
        return request;
    }
}