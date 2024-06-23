// Copyright 2010 Nejat Dilek  <imruon@gmail.com>
// Licensed under the terms of the GNU GPL, version 2
// http://www.gnu.org/licenses/old-licenses/gpl-2.0.txt
using System;
using System.Net;
using System.IO;
using System.Threading;
using System.Net.Cache;

namespace WiiGSC
{
    public delegate void DownloadProgressHandler(int bytesRead, int totalBytes);

    // The RequestState class passes data across async calls.
    public class DownloadInfo
    {
        const int BufferSize = 1024;
        public byte[] BufferRead;

        public bool useFastBuffers;
        public byte[] dataBufferFast;
        public System.Collections.ArrayList dataBufferSlow;

        public int dataLength;
        public int bytesProcessed;

        public WebRequest Request;
        public Stream ResponseStream;

        public DownloadProgressHandler ProgressCallback;
        public DownloadErrorHandler ErrorCallback;

        public DownloadInfo()
        {
            BufferRead = new byte[BufferSize];
            Request = null;
            dataLength = -1;
            bytesProcessed = 0;

            useFastBuffers = true;
        }
    }

    // ClientGetAsync issues the async request.
    public class WebDownload
    {
        public WebProxy proxy;
        public ManualResetEvent allDone = new ManualResetEvent(false);
        const int BUFFER_SIZE = 1024;

        public byte[] Download(string url, DownloadProgressHandler progressCB, DownloadErrorHandler errorHandler)
        {
            try
            {
                // Ensure flag set correctly.			
                allDone.Reset();

                // Get the URI from the command line.
                Uri httpSite = new Uri(url);

                // Create the request object.
                WebRequest req = WebRequest.Create(httpSite);

                WebProxy proxy = AppUtils.GetProxyIfRequired();
                req.Proxy = (proxy != null) ? proxy : req.Proxy;


                HttpRequestCachePolicy policy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
                req.CachePolicy = policy;

                // Create the state object.
                DownloadInfo info = new DownloadInfo();

                // Put the request into the state object so it can be passed around.
                info.Request = req;

                // Assign the callbacks
                info.ProgressCallback += progressCB;
                info.ErrorCallback += errorHandler;

                // Issue the async request.
                IAsyncResult r = (IAsyncResult)req.BeginGetResponse(new AsyncCallback(ResponseCallback), info);

                // Wait until the ManualResetEvent is set so that the application
                // does not exit until after the callback is called.
                allDone.WaitOne();

                // Pass back the downloaded information.

                if (info.useFastBuffers)
                    return info.dataBufferFast;
                else
                {
                    byte[] data = new byte[info.dataBufferSlow.Count];
                    for (int b = 0; b < info.dataBufferSlow.Count; b++)
                        data[b] = (byte)info.dataBufferSlow[b];
                    return data;
                }
            }
            catch (Exception ex)
            {
                errorHandler.Invoke(ex.Message);
                return null;
            }
        }

        private void ResponseCallback(IAsyncResult ar)
        {
            // Get the DownloadInfo object from the async result were
            // we're storing all of the temporary data and the download
            // buffer.
            DownloadInfo info = (DownloadInfo)ar.AsyncState;

            try
            {
                // Get the WebRequest from RequestState.
                WebRequest req = info.Request;

                // Call EndGetResponse, which produces the WebResponse object
                // that came from the request issued above.
                WebResponse resp = req.EndGetResponse(ar);

                // Find the data size from the headers.
                string strContentLength = resp.Headers["Content-Length"];
                if (strContentLength != null)
                {
                    info.dataLength = Convert.ToInt32(strContentLength);
                    info.dataBufferFast = new byte[info.dataLength];
                }
                else
                {
                    info.useFastBuffers = false;
                    info.dataBufferSlow = new System.Collections.ArrayList(BUFFER_SIZE);
                }

                //  Start reading data from the response stream.
                Stream ResponseStream = resp.GetResponseStream();

                // Store the response stream in RequestState to read
                // the stream asynchronously.
                info.ResponseStream = ResponseStream;

                //  Pass do.BufferRead to BeginRead.
                IAsyncResult iarRead = ResponseStream.BeginRead(info.BufferRead,
                    0,
                    BUFFER_SIZE,
                    new AsyncCallback(ReadCallBack),
                    info);
            }
            catch (Exception ex)
            {
                info.ErrorCallback.Invoke(ex.Message);                
            }
        }

        private void ReadCallBack(IAsyncResult asyncResult)
        {
            // Get the DownloadInfo object from AsyncResult.
            DownloadInfo info = (DownloadInfo)asyncResult.AsyncState;

            try
            {
                // Retrieve the ResponseStream that was set in RespCallback.
                Stream responseStream = info.ResponseStream;

                // Read info.BufferRead to verify that it contains data.
                int bytesRead = responseStream.EndRead(asyncResult);
                if (bytesRead > 0)
                {
                    if (info.useFastBuffers)
                    {
                        System.Array.Copy(info.BufferRead, 0,
                            info.dataBufferFast, info.bytesProcessed,
                            bytesRead);
                    }
                    else
                    {
                        for (int b = 0; b < bytesRead; b++)
                            info.dataBufferSlow.Add(info.BufferRead[b]);
                    }
                    info.bytesProcessed += bytesRead;

                    // If a registered progress-callback, inform it of our
                    // download progress so far.
                    if (info.ProgressCallback != null)
                        info.ProgressCallback(info.bytesProcessed, info.dataLength);

                    // Continue reading data until responseStream.EndRead returns –1.
                    IAsyncResult ar = responseStream.BeginRead(
                        info.BufferRead, 0, BUFFER_SIZE,
                        new AsyncCallback(ReadCallBack), info);
                }
                else
                {
                    responseStream.Close();
                    allDone.Set();
                }
            }
            catch (Exception ex)
            {
                info.ErrorCallback.Invoke(ex.Message);
            }
            return;
        }
    }
}
