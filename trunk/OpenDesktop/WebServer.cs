// OpenDesktop - A search tool for the Windows desktop
// Copyright (C) 2005-2006, Pravin Paratey (pravinp at gmail dot com)
// http://opendesktop.berlios.de
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Collections;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Collections.Specialized;

using OpenDesktop.Properties;

namespace OpenDesktop
{
    enum HttpResponseCode : int
    {
        Ok = 200,
        InvalidRequest = 400,
        FileNotFound = 404,
        Conflict = 409,
        Unsupported = 415
    }

    #region Class ContentType
    public sealed class ContentType
    {
        /// <summary>
        /// returns text/html
        /// </summary>
        public static string HTML
        {
            get { return "text/html;charset=utf-8"; }
        }
        /// <summary>
        /// returns text/css
        /// </summary>
        public static string CSS
        {
            get { return "text/css"; }
        }
        /// <summary>
        /// returns image/jpg
        /// </summary>
        public static string IMAGE_JPEG
        {
            get { return "image/jpg"; }
        }
        /// <summary>
        /// returns image/gif
        /// </summary>
        public static string IMAGE_GIF
        {
            get { return "image/gif"; }
        }
        /// <summary>
        /// returns image/png
        /// </summary>
        public static string IMAGE_PNG
        {
            get { return "image/png"; }
        }
        /// <summary>
        /// returns image/icon
        /// </summary>
        public static string IMAGE_ICO
        {
            get { return "image/icon"; }
        }
    }
    #endregion

    /// <summary>
    /// WebServer class. This class is responsible for:
    /// 1. Creating a server and listening for connections.
    /// 2. On connect, extract GET or POST string.
    /// 3. Open target page and parse it.
    /// 4. While parsing, call appropriate functions and create output page.
    /// 5. Display output page.
    /// </summary>
    class WebServer
    {
        #region Member Variables
        private TcpListener m_tcpListener;
        private static WebServer _self;
        private static object m_lock = new object();
        private bool m_bDie;
        private Thread m_objListenThread;
        private string m_strLocalAddress;
        private bool m_bIsRunning;
        private string m_strWebRoot;
        #endregion

        #region Constructor
        private WebServer()
        {
            try
            {
                m_strWebRoot = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), @"WebRoot");
            }
            catch
            {
                m_strWebRoot = string.Empty;
            }
        }

        public static WebServer Instance
        {
            get
            {
                lock (m_lock)
                {
                    if (_self == null)
                    {
                        _self = new WebServer();
                    }
                }
                return _self;
            }
        }
        #endregion

        #region Start/Stop Server
        public void Start()
        {
            lock (m_lock)
            {
                if (m_bIsRunning)
                    return;
                m_bIsRunning = true;
            }
            m_tcpListener = new TcpListener(IPAddress.Loopback, 0); // AnyPort
            m_tcpListener.Start();
            m_strLocalAddress = string.Format("http://{0}/", m_tcpListener.LocalEndpoint.ToString());

            m_objListenThread = new Thread(new ThreadStart(ListenThread));
            m_objListenThread.Start();
        }

        public void Stop()
        {
            if (!m_bIsRunning)
                return;

            m_bDie = true;
            m_tcpListener.Stop();

            lock (m_lock)
            {
                m_bIsRunning = false;
            }
        }
        #endregion

        #region Properties
        public string LocalAddress
        {
            get
            {
                return m_strLocalAddress;
            }
        }

        public bool IsRunning
        {
            get
            {
                return m_bIsRunning;
            }
        }
        #endregion

        void ListenThread()
        {
            m_bDie = false;
            while (!m_bDie)
            {
                Socket theClient;
                try
                {
                    theClient = m_tcpListener.AcceptSocket();
                    if (theClient.Connected)
                    {
                        ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessRequest), theClient);
                    }
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode != SocketError.Interrupted)
                    {
                        Logger.Instance.LogError("Client interrupted while handling request");
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        void ProcessRequest(object socket)
        {
            Socket sock = socket as Socket;
            if (sock == null)
                return;
            byte[] inbuf = new byte[512]; // README: If Webserver fails with "server busy", increase this size
            sock.Receive(inbuf);
            string strRequest = Encoding.ASCII.GetString(inbuf);
            NameValueCollection queryString = null;
            Uri uriRequest = ValidateRequest(strRequest);
            
            Logger.Instance.LogDebug(uriRequest.ToString());
            
            if (uriRequest == null)
            {
                Logger.Instance.LogDebug("Invalid Request" + strRequest);
                SendErrorResponse(ref sock, HttpResponseCode.InvalidRequest, "Invalid request:\r\n" + strRequest);
                return;
            }

            Logger.Instance.LogDebug("Processing Request: " + uriRequest.ToString());
            
            if (uriRequest.Query.Length > 0)
            {
               queryString = MakeQueryString(uriRequest.Query);
            }
            string strPathFragment = uriRequest.LocalPath.Replace('/', '\\');
            if (strPathFragment.Equals("\\"))
                strPathFragment = "index.html";
            else if (strPathFragment[0] == '\\')
                strPathFragment = strPathFragment.Substring(1);
            string strFilePath = Path.Combine(m_strWebRoot, strPathFragment);

            if (!File.Exists(strFilePath))
            {
                SendErrorResponse(ref sock, HttpResponseCode.FileNotFound, Resources.FileNotFound);
                return;
            }

            BinaryReader reader = null;
            byte[] responseData;
            try
            {
                FileInfo finfo = new FileInfo(strFilePath);
                reader = new BinaryReader(finfo.OpenRead());
                responseData = reader.ReadBytes((int)finfo.Length);
                reader.Close();
            }
            catch (IOException e)
            {
                Logger.Instance.LogException(e);
                if (reader != null)
                {
                    reader.Close();
                    reader = null;
                }
                SendErrorResponse(ref sock, HttpResponseCode.Conflict, e.ToString());
                return;
            }
            string strContentType;
            string strExtention = strFilePath.Substring(strFilePath.Length - 4);
            if (strExtention.Equals(".htm", StringComparison.InvariantCultureIgnoreCase) 
                || strExtention.Equals("html", StringComparison.InvariantCultureIgnoreCase))
            {
                strContentType = ContentType.HTML;

                string strFileData = Encoding.UTF8.GetString(responseData);
                ReplaceTag replaceTag = new ReplaceTag(queryString);
                MatchEvaluator matchTag = new MatchEvaluator(replaceTag.Replace);
                strFileData = Regex.Replace(strFileData, "<\\$([^>]+)\\$>", matchTag);
                responseData = Encoding.UTF8.GetBytes(strFileData);
            }
            else if (strExtention.Equals(".jpg", StringComparison.InvariantCultureIgnoreCase)
                || strExtention.Equals("jpeg", StringComparison.InvariantCultureIgnoreCase))
            {
                strContentType = ContentType.IMAGE_JPEG;
            }
            else if (strExtention.Equals(".png", StringComparison.InvariantCultureIgnoreCase))
            {
                strContentType = ContentType.IMAGE_PNG;
            }
            else if (strExtention.Equals(".gif", StringComparison.InvariantCultureIgnoreCase))
            {
                strContentType = ContentType.IMAGE_GIF;
            }
            else if (strExtention.Equals(".css", StringComparison.InvariantCultureIgnoreCase))
            {
                strContentType = ContentType.CSS;
            }
            else if (strExtention.Equals(".ico", StringComparison.InvariantCultureIgnoreCase))
            {
                strContentType = ContentType.IMAGE_ICO;
            }
            else
            {
                SendErrorResponse(ref sock, HttpResponseCode.Unsupported, "Unsupported mime type requested");
                return;
            }

            SendResponse(ref sock, responseData, strContentType, responseData.Length);
        }

        /// <summary>
        /// Creates a HTTP 1.1 header string to return to browsers
        /// </summary>
        /// <param name="status">Status Number ex. 200 for OK, 404 for file not found</param>
        /// <param name="contentType">content type of the response body</param>
        /// <param name="contentLength">length of response body</param>
        /// <returns>header string</returns>
        string CreateHeader(HttpResponseCode status, string contentType, int contentLength)
        {
            const string CRLF = "\r\n";
            string strHeader =
                "HTTP/1.1 " + ((int)status).ToString() + CRLF +
                "Server: " + Resources.ServerString + CRLF +
                "Content-Type: " + contentType + CRLF +
                "Accept-Ranges: bytes" + CRLF +
                "Content-Length: " + contentLength + CRLF + CRLF;
            return strHeader;
        }

        void SendResponse(ref Socket sock, byte[] content, string contentType, int contentLength)
        {
            string strHeader = CreateHeader(HttpResponseCode.Ok, contentType, contentLength);
            try
            {
                sock.Send(Encoding.ASCII.GetBytes(strHeader));
                sock.Send(content, contentLength, SocketFlags.None);
            }
            catch { }
            // FIXME: The sleep has been added so that the data reaches the browser before
            // we shutdown the socket. What is strange is Send is a sync call and so the
            // control should sock.Shutdown only after the Send sends data to the browser
            // But this is not what happens. Hence this Sleep hack.
            Thread.Sleep(100);
            sock.Shutdown(SocketShutdown.Both);
            sock.Close(); sock = null;
        }

        /// <summary>
        /// Sends an error response on the socket. Also closes the socket
        /// </summary>
        /// <param name="sock">socket to send response to</param>
        /// <param name="status">Error Code</param>
        /// <param name="errMessage">Message</param>
        void SendErrorResponse(ref Socket sock, HttpResponseCode status, string errMessage)
        {
            StreamReader reader;
            string strErrorPage;
            try
            {
                string strFilePath = Path.Combine(m_strWebRoot, "error.html");
                reader = new StreamReader(strFilePath, Encoding.UTF8);
                strErrorPage = reader.ReadToEnd();
                reader.Close(); reader = null;

                strErrorPage = strErrorPage.Replace("<$ErrorMessage$>", errMessage);
            }
            catch
            {
                strErrorPage = errMessage;
            }

            byte [] outbuf = Encoding.UTF8.GetBytes(strErrorPage);
            string strHeader = CreateHeader(status, "text/html;charset=utf-8", outbuf.Length);
            // Header is always in ASCII encoding
            try
            {
                sock.Send(Encoding.ASCII.GetBytes(strHeader));
                sock.Send(outbuf, outbuf.Length, SocketFlags.None);
            }
            catch { }
            sock.Shutdown(SocketShutdown.Both);
            sock.Close(); sock = null;
        }

        NameValueCollection MakeQueryString(string queryString)
        {
            if (queryString.StartsWith("?"))
                queryString = queryString.Substring(1);
            string[] strQueryTerms = queryString.Split('&');
            if (strQueryTerms.Length < 1)
                return null;
            NameValueCollection objQueryString = new NameValueCollection(strQueryTerms.Length);
            foreach (string strTerm in strQueryTerms)
            {
                string[] strValuePair = strTerm.Split('=');
                if (strValuePair.Length > 0)
                {
                    objQueryString.Add(strValuePair[0], strValuePair[1]);
                }
            }
            return objQueryString;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Returns uri object on success else null</returns>
        Uri ValidateRequest(string request)
        {
            string strUrl = null;
            if (request.StartsWith("GET", StringComparison.OrdinalIgnoreCase))
            {
                // Do Get
                // Get the url and parse it
                int iStart = request.IndexOf(' ') + 1;
                int iEnd = request.IndexOf(' ', iStart);

                strUrl = request.Substring(iStart, iEnd - iStart);
            }
            else if (request.StartsWith("POST", StringComparison.OrdinalIgnoreCase))
            {
                // Do Post
                // TODO: Content-Type field is ignored for now.
                // Later, you'd want to use the content-type to determine if the data
                // that follows is of type text, else flag an error
                int iStart = request.IndexOf("\r\n\r\n") + 4;
                if (iStart > 4)
                {
                    strUrl = request.Substring(iStart);
                }
            }
            
            if(strUrl == null)
            {
                return null;
            }
            
            Uri uriRequest = null;
            try
            {
                uriRequest = new Uri(new Uri(m_strLocalAddress), strUrl);
            }
            catch
            {
                return null;
            }
            return uriRequest;
        }
    }
}
