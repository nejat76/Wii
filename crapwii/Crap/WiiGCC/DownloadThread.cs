// Copyright 2010 Nejat Dilek  <imruon@gmail.com>
// Licensed under the terms of the GNU GPL, version 2
// http://www.gnu.org/licenses/old-licenses/gpl-2.0.txt
using System;
using System.Net;

namespace WiiGSC
{
    public delegate void DownloadCompleteHandler(byte[] dataDownloaded, string[] callbackArgs);
    public delegate void DownloadErrorHandler(string exception);

    /// <summary>
    /// Summary description for DownloadThread.
    /// </summary>
    public class DownloadThread
    {
        private string[] callbackArgs;


        public event DownloadCompleteHandler CompleteCallback;
        public event DownloadProgressHandler ProgressCallback;
        public event DownloadErrorHandler ErrorCallback;
        public WebProxy Proxy;

        public string _downloadUrl = "";
        public string DownloadUrl
        {
            get
            {
                return _downloadUrl;
            }
            set
            {
                _downloadUrl = value;
            }
        }

        public void Download(object callbackArgs)
        {
            if (CompleteCallback != null &&
                  DownloadUrl != "")
            {
                this.callbackArgs = (string[]) callbackArgs;
                WebDownload webDL = new WebDownload();
                webDL.proxy = Proxy;
                byte[] downloadedData = webDL.Download(DownloadUrl, ProgressCallback, ErrorCallback);
                CompleteCallback(downloadedData, this.callbackArgs);
            }
        }
    }
}
