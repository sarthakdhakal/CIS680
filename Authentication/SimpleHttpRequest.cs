﻿using System.Net;
using System.Text;

namespace NewDotnet.Authentication
{
    public class SimpleHttpRequest
    {
        public string LastResponse { get; set; }
        private readonly CookieContainer cookies = new CookieContainer();

        internal string GetCookieValue(Uri SiteUri, string name)
        {
            Cookie cookie = cookies.GetCookies(SiteUri)[name];
            return cookie?.Value;
        }

        public string GetResponseContent(HttpWebResponse response)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }
            Stream dataStream = null;
            StreamReader reader = null;
            string responseFromServer = null;

            try
            {
                // Get the stream containing content returned by the server.
                dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                reader = new StreamReader(dataStream);
                // Read the content.
                responseFromServer = reader.ReadToEnd();
                // Cleanup the streams and the response.
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                reader?.Close();
                dataStream?.Close();
                response.Close();
            }
            LastResponse = responseFromServer;
            return responseFromServer;
        }
        public HttpWebResponse SendPOSTRequest(string uri, string content, string login, string password, bool allowAutoRedirect)
        {
            HttpWebRequest request = GeneratePOSTRequest(uri, content, login, password, allowAutoRedirect);
            return GetResponse(request);
        }

        public HttpWebResponse SendGETRequest(string uri, string login, string password, bool allowAutoRedirect)
        {
            HttpWebRequest request = GenerateGETRequest(uri, login, password, allowAutoRedirect);
            return GetResponse(request);
        }

        public HttpWebResponse SendRequest(string uri, string content, string method, string login, string password, bool allowAutoRedirect)
        {
            HttpWebRequest request = GenerateRequest(uri, content, method, login, password, allowAutoRedirect);
            return GetResponse(request);
        }

        public HttpWebRequest GenerateGETRequest(string uri, string login, string password, bool allowAutoRedirect)
        {
            return GenerateRequest(uri, null, "GET", null, null, allowAutoRedirect);
        }

        public HttpWebRequest GeneratePOSTRequest(string uri, string content, string login, string password, bool allowAutoRedirect)
        {
            return GenerateRequest(uri, content, "POST", null, null, allowAutoRedirect);
        }

        internal HttpWebRequest GenerateRequest(string uri, string content, string method, string login, string password, bool allowAutoRedirect)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }
            // Create a request using a URL that can receive a post. 
            HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;
            // Set the Method property of the request to POST.
            request.Method = method;
            // Set cookie container to maintain cookies
            request.CookieContainer = cookies;
            request.AllowAutoRedirect = allowAutoRedirect;
            // If login is empty use defaul credentials
            request.Credentials = string.IsNullOrEmpty(login) ? CredentialCache.DefaultNetworkCredentials : new NetworkCredential(login, password);
            if (method == "POST")
            {
                // Convert POST data to a byte array.
                byte[] byteArray = Encoding.UTF8.GetBytes(content);
                // Set the ContentType property of the WebRequest.
                request.ContentType = "application/x-www-form-urlencoded";
                // Set the ContentLength property of the WebRequest.
                request.ContentLength = byteArray.Length;
                // Get the request stream.
                Stream dataStream = request.GetRequestStream();
                // Write the data to the request stream.
                dataStream.Write(byteArray, 0, byteArray.Length);
                // Close the Stream object.
                dataStream.Close();
            }
            return request;
        }

        internal HttpWebResponse GetResponse(HttpWebRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                cookies.Add(response.Cookies);
            }
            catch (WebException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Web exception occurred. Status code: {ex.Status}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return response;
        }

    }
}