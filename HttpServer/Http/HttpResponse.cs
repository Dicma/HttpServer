﻿using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Feri.MS.Http
{
    /// <summary>
    /// Class handles everything related to server response to client. It is related to HttpRequest and HttpServer.
    /// It is created by HttpServer from HttpRequest object. All data writen by write method goes to the requesting stream.
    /// Allows to add cookies and headers to the server response. It allso automatically adds session cookie if there is session.
    /// </summary>
    public class HttpResponse
    {
        // Todo atributi (headers), redirekti, ...

        #region Declarations
        private Stream _stream;
        private Dictionary<string, string> _headers = new Dictionary<string, string>();
        private Dictionary<string, HttpCookie> _cookies = new Dictionary<string, HttpCookie>();

        private SessionManager _sessionManager;

        private HttpRequest _request;

        public bool _debug = false;
        #endregion

        #region Properties
        internal SessionManager SessionManager
        {
            get
            {
                return _sessionManager;
            }

            set
            {
                _sessionManager = value;
            }
        }
        #endregion

        public HttpResponse(HttpRequest request)
        {
            _request = request;
            _stream = _request.Output;
        }


        /// <summary>
        /// Metod formates HTTP Response and adds user data to it.
        /// It can add status code (200 OK by default), headers (by default it adds session cookie if there is session)
        /// </summary>
        /// <param name="data">byte[] representing data to display to the user (HTML page, JSON object, ...)</param>
        /// <param name="contentType">Content type of data that is send. Needed by the client to display it correctly.</param>
        /// <param name="statusCode">Optional parameter, usualy it is ok to leave it empty, unless you need to display server message to the client, like 404, 401, ...</param>
        /// <param name="headers">Optional paarameter, used to add custom headers. Usualy it is ok to leave empty, used by some server responses to add ionformation for client.</param>
        public void Write(byte[] data, string contentType, string statusCode = "200 OK", Dictionary<string, string> headers = null)
        {
            //try
            //{
            // Show the html 
            string _eol = "\r\n";
            string htmlVersion = "HTTP/1.1 ";
            StringBuilder _header = new StringBuilder();

            _header.Append(htmlVersion + statusCode + _eol);
            if (headers != null)  // Obdelamo headerje podane pri klicu
                foreach (KeyValuePair<string, string> par in headers)
                {
                    _header.Append(par.Key + ": " + par.Value + _eol);
                }
            if (_headers.Count > 0)  // obdelamo headerje podane preko .AddHeader() metode
                foreach (KeyValuePair<string, string> par in _headers)
                {
                    _header.Append(par.Key + ": " + par.Value + _eol);
                }
            if (_request.SessionUpdated)
            {
                HttpCookie _sessionCookie = _request.GetCookie("SessionID");
                if (_sessionCookie != null)
                {
                    _cookies.Add("SessionID", _sessionCookie);
                }
            }
            if (_cookies.Count > 0) // Obdelamo cookije...
            {
                //_header.Append("Set-Cookie: ");
                int _cookieIterration = 1;
                foreach (KeyValuePair<string, HttpCookie> cookie in _cookies)
                {
                    _header.Append("Set-Cookie: ");
                    if (!cookie.Value.HasKeys) // Cookie nima SubCookijev
                    {
                        if (cookie.Value.Value != null)
                        {
                            _header.Append(WebUtility.UrlEncode(cookie.Value.Name) + "=" + WebUtility.UrlEncode(cookie.Value.Value));
                        }
                        else
                        {
                            _header.Append(WebUtility.UrlEncode(cookie.Value.Name));
                        }
                    }
                    else
                    {
                        int _subCookieIterration = 1;
                        foreach (KeyValuePair<string, string> par in cookie.Value.Values)
                        {
                            if (par.Value != null)
                            {
                                _header.Append(WebUtility.UrlEncode(par.Key) + "=" + WebUtility.UrlEncode(par.Value));
                            }
                            else
                            {
                                _header.Append(WebUtility.UrlEncode(par.Key));
                            }
                            if (_subCookieIterration < cookie.Value.Values.Count)
                            {
                                _header.Append("&");
                            }
                            _subCookieIterration++;
                        }
                        // Obdelamo subcookije
                    }
                    if (cookie.Value.Path != null)
                    {
                        _header.Append("; path=" + cookie.Value.Path);
                    }
                    if (cookie.Value.Expire.HasValue)
                    {
                        _header.Append("; expires=" + cookie.Value.Expire.Value.ToString("R"));
                    }
                    if (_cookieIterration < _cookies.Count)
                    {
                        //_header.Append("; ");
                        _header.Append(_eol);
                    }
                    _cookieIterration++;
                }
                _header.Append(_eol);
            }
            _header.Append("Status: " + statusCode + _eol);
            _header.Append("Server: MihaServer/0.1 (Windows RT)" + _eol);
            _header.Append("X-Powered-By: WindowsRT" + _eol);
            _header.Append("TSV: !" + _eol);
            _header.Append("Content-Type: " + contentType + _eol);
            _header.Append("Content-Length: " + data.Length + _eol);
            _header.Append("Connection: close" + _eol + _eol);

            byte[] headerArray = Encoding.UTF8.GetBytes(_header.ToString());
            _stream.Write(headerArray, 0, headerArray.Length);
            _stream.Write(data, 0, data.Length);
            _stream.Flush();
            //}
            //catch (Exception e)
            //{
            //    Debug.WriteLine(e.Message);
            //    Debug.WriteLine(e.StackTrace);
            //}

        }

        /// <summary>
        /// Metod for adding custom headers. Header is added without ":", it is added automatically when header is constructed.
        /// </summary>
        /// <param name="header">Name of hadder to add. Must be unique.</param>
        /// <param name="data">Data for the header</param>
        /// <returns>Returns true if header was added, false if it was not. This means that header with same name exists.</returns>
        public bool AddHeader(string header, string data)
        {
            //try
            //{
            if (!_headers.ContainsKey(header))
            {
                _headers.Add(header, data);
                return true;
            }
            else
            {
                return false;
            }
            //}
            //catch (Exception e)
            //{
            //    Debug.WriteLine(e.Message);
            //    Debug.WriteLine(e.StackTrace);
            //    return false;
            //}
        }

        /// <summary>
        /// Metod for removing custom headers. Header name is without ":" 
        /// </summary>
        /// <param name="header">Name of header to remove.</param>
        /// <returns>Returns true if header was removed, false if it was not. This means header did not exists in the response.</returns>
        public bool RemoveHeader(string header)
        {
            //try
            //{
            if (_headers.ContainsKey(header))
            {
                _headers.Remove(header);
                return true;
            }
            else
            {
                return false;
            }
            //}
            //catch (Exception e)
            //{
            //    Debug.WriteLine(e.Message);
            //    Debug.WriteLine(e.StackTrace);
            //    return false;
            //}
        }

        /// <summary>
        /// Method returns current value of the header. Header name is without ":"
        /// </summary>
        /// <param name="header">Name of header to return.</param>
        /// <returns>Returns string representation of the header value or null if header did not exist.</returns>
        public string GetHeader(string header)
        {
            //try
            //{
            if (_headers.ContainsKey(header))
            {
                return _headers[header].ToString();
            }
            else
            {
                return null;
            }
            //}
            //catch (Exception e)
            //{
            //    Debug.WriteLine(e.Message);
            //    Debug.WriteLine(e.StackTrace);
            //    return null;
            //}
        }

        /// <summary>
        /// Metod to quickly check if cookie exists in the response.
        /// </summary>
        /// <param name="name">Name of the cookie we are looking for.</param>
        /// <returns>Return TRUE if cookie exists in current request and FALSE if it does not.</returns>
        public bool ContainsCookie(string name)
        {
            //try
            //{
            return _cookies.ContainsKey(name);
            //}
            //catch (Exception e)
            //{
            //    Debug.WriteLine(e.Message);
            //    Debug.WriteLine(e.ToString());
            //    return false;
            //}
        }

        /// <summary>
        /// Metod for adding custom cookie to the HTTP resposne. It must not exist in the response.
        /// </summary>
        /// <param name="cookie">HttpCookie to add</param>
        /// <returns>True if cookie was added and false if cookie allready exists in the response.</returns>
        public bool AddCookie(HttpCookie cookie)
        {
            //try
            //{
            if (!_cookies.ContainsKey(cookie.Name))
            {
                _cookies.Add(cookie.Name, cookie);
                return true;
            }
            else
                return false;
            //}
            //catch (Exception e)
            //{
            //    Debug.WriteLine(e.Message);
            //    Debug.WriteLine(e.ToString());
            //    return false;
            //}
        }

        /// <summary>
        /// Method for reading cookie in the current response.
        /// </summary>
        /// <param name="name">String name of the cookie in the repsosne (HttpCookie.Name)</param>
        /// <returns>Returns HttpCookie or null if it does not exist in response.</returns>
        public HttpCookie GetCookie(string name)
        {
            //try
            //{
            if (_cookies.ContainsKey(name))
            {
                return _cookies[name];
            }
            else
                return null;
            //}
            //catch (Exception e)
            //{
            //    Debug.WriteLine(e.Message);
            //    Debug.WriteLine(e.ToString());
            //    return null;
            //}
        }
    }
}
