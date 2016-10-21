using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AzureBlobUpload
{
    public class WebRequestHelper
    {

        public event Action<long> OnStart;
        public event Action<long> OnProgress;

        public string GetStringUtf8(string url, string userAgent, byte[] data = null, int timeout = 30000, string contentType = null, Dictionary<HttpRequestHeader, string> httpRequestHeader = null)
        {
            byte[] result = GetData(url, userAgent, data, timeout, contentType, httpRequestHeader);
            if (result == null)
                return null;

            return Encoding.UTF8.GetString(result);
        }

        public Task<string> GetStringUtf8Async(string searchUrl, string userAgent, byte[] data = null, int timeout = 30000, string contentType = null, Dictionary<HttpRequestHeader, string> httpRequestHeader = null)
        {
            return Task.Factory.StartNew(() => GetStringUtf8(searchUrl, userAgent, data, timeout, contentType, httpRequestHeader));
        }

        public Task<byte[]> GetDataAsync(string url, string userAgent, byte[] data = null, int timeout = 30000, string contentType = null, Dictionary<HttpRequestHeader, string> httpRequestHeader = null)
        {
            return Task.Factory.StartNew(() => GetData(url, userAgent, data, timeout, contentType, httpRequestHeader));
        }

        public byte[] GetData(string url, string userAgent, byte[] data = null, int timeout = 30000, string contentType = null, Dictionary<HttpRequestHeader, string> httpRequestHeader = null)
        {
            if (url == null)
                return null;

            Uri uri = new Uri(url);
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(uri);

            if (string.IsNullOrEmpty(userAgent) == false)
            {
                webRequest.UserAgent = userAgent;
            }

            if (httpRequestHeader != null)
            {
                foreach (var pair in httpRequestHeader)
                {
                    if (pair.Key == HttpRequestHeader.Accept)
                    {
                        webRequest.Accept = pair.Value;
                    }
                }
            }

            webRequest.Timeout = timeout;

            if (data != null)
            {
                webRequest.Method = "POST";
                webRequest.ContentType = contentType;
                webRequest.ContentLength = data.Length;

                using (Stream newStream = webRequest.GetRequestStream())
                {
                    // Send the data.
                    newStream.Write(data, 0, data.Length);
                }
            }

            using (WebResponse response = webRequest.GetResponse())
            {
                Stream stream = response.GetResponseStream();
                return ReadFully(stream, response.ContentLength);
            }
        }


        /// <summary>
        /// Reads data from a stream until the end is reached. The
        /// data is returned as a byte array. An IOException is
        /// thrown if any of the underlying IO calls fail.
        /// </summary>
        /// <param name="stream">The stream to read data from</param>
        /// <param name="initialLength">The initial buffer length</param>
        public byte[] ReadFully(Stream stream, long initialLength)
        {
            if (OnStart != null)
                OnStart(initialLength);

            // If we've been passed an unhelpful initial length, just
            // use 32K.
            if (initialLength < 1)
            {
                initialLength = 32768;
            }

            byte[] buffer = new byte[initialLength];
            int read = 0;

            int chunk;
            while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
            {
                read += chunk;

                if (OnProgress != null)
                    OnProgress(read);

                // If we've reached the end of our buffer, check to see if there's
                // any more information
                if (read == buffer.Length)
                {
                    int nextByte = stream.ReadByte();

                    // End of stream? If so, we're done
                    if (nextByte == -1)
                    {
                        return buffer;
                    }

                    // Nope. Resize the buffer, put in the byte we've just
                    // read, and continue
                    byte[] newBuffer = new byte[buffer.Length * 2];
                    Array.Copy(buffer, newBuffer, buffer.Length);
                    newBuffer[read] = (byte)nextByte;
                    buffer = newBuffer;
                    read++;
                }
            }
            // Buffer is now too big. Shrink it.
            byte[] ret = new byte[read];
            Array.Copy(buffer, ret, read);
            return ret;
        }
    }
}