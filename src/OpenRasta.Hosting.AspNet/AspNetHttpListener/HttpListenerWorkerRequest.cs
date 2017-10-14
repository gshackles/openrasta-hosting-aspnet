﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Web;

namespace OpenRasta.Hosting.AspNet.AspNetHttpListener
{
  public class HttpListenerWorkerRequest : HttpWorkerRequest
  {
    readonly HttpListenerContext _context;
    readonly string _physicalDir;
    readonly string _virtualDir;

    public HttpListenerWorkerRequest(
      HttpListenerContext context, string vdir, string pdir)
    {
      if (null == context)
        throw new ArgumentNullException("context");
      if (null == vdir || vdir.Equals(string.Empty))
        throw new ArgumentException("vdir");
      if (null == pdir || pdir.Equals(string.Empty))
        throw new ArgumentException("pdir");

      _context = context;
      _virtualDir = vdir;
      _physicalDir = pdir;
      _context.Response.SendChunked = false;
    }

    public override void CloseConnection()
    {
      // _context.Close();
    }

    public override void EndOfRequest()
    {
      _context.Response.Close();
    }

    public override void FlushResponse(bool finalFlush)
    {
      _context.Response.OutputStream.Flush();
    }

    public override string GetAppPath()
    {
      return _virtualDir;
    }

    public override string GetAppPathTranslated()
    {
      return _physicalDir;
    }

    public override string GetFilePath()
    {
      return _context.Request.Url.LocalPath;
    }

    public override string GetFilePathTranslated()
    {
      string s = GetFilePath();
      s = s.Substring(_virtualDir.Length);
      s = s.Replace('/', Path.DirectorySeparatorChar);
      return Path.Combine(_physicalDir, s);
    }

    public override string GetHttpVerbName()
    {
      return _context.Request.HttpMethod;
    }

    public override string GetHttpVersion()
    {
      return string.Format("HTTP/{0}.{1}",
        _context.Request.ProtocolVersion.Major,
        _context.Request.ProtocolVersion.Minor);
    }

    public override string MapPath(string virtualPath)
    {
      var fsPath = virtualPath.Replace('/', Path.DirectorySeparatorChar);
      fsPath = fsPath[0] == Path.DirectorySeparatorChar ? fsPath.Substring(1) : fsPath;
      var mapped = Path.Combine(
        _physicalDir,
        fsPath);
      return mapped;
    }

    public override string GetKnownRequestHeader(int index)
    {
      switch (index)
      {
        case HeaderUserAgent:
          return _context.Request.UserAgent;
        default:
          return _context.Request.Headers[GetKnownRequestHeaderName(index)];
      }
    }

    public override string GetLocalAddress()
    {
      return _context.Request.LocalEndPoint.Address.ToString();
    }

    public override int GetLocalPort()
    {
      return _context.Request.LocalEndPoint.Port;
    }

    public override string GetPathInfo()
    {
      return string.Empty;
    }

    public override string GetQueryString()
    {
      string queryString = string.Empty;
      string rawUrl = _context.Request.RawUrl;
      int index = rawUrl.IndexOf('?');
      if (index != -1)
        queryString = rawUrl.Substring(index + 1);
      return queryString;
    }

    public override string GetRawUrl()
    {
      return _context.Request.RawUrl;
    }

    public override string GetRemoteAddress()
    {
      return _context.Request.RemoteEndPoint.Address.ToString();
    }

    public override int GetRemotePort()
    {
      return _context.Request.RemoteEndPoint.Port;
    }

    public override string GetServerVariable(string name)
    {
      // TODO: vet this list
      switch (name)
      {
        case "HTTPS":
          return _context.Request.IsSecureConnection ? "on" : "off";
        case "HTTP_USER_AGENT":
          return _context.Request.Headers["UserAgent"];
        case "HTTP_HOST":
          return _context.Request.Headers["Host"];
        default:
          return null;
      }
    }

    public override string GetUnknownRequestHeader(string name)
    {
      return _context.Request.Headers[name];
    }

    public override string[][] GetUnknownRequestHeaders()
    {
      string[][] unknownRequestHeaders;
      var headers = _context.Request.Headers;
      int count = headers.Count;
      var headerPairs = new List<string[]>(count);
      for (int i = 0; i < count; i++)
      {
        string headerName = headers.GetKey(i);
        if (GetKnownRequestHeaderIndex(headerName) == -1)
        {
          string headerValue = headers.Get(i);
          headerPairs.Add(new[] {headerName, headerValue});
        }
      }
      unknownRequestHeaders = headerPairs.ToArray();
      return unknownRequestHeaders;
    }

    public override string GetUriPath()
    {
      return _context.Request.Url.LocalPath;
    }

    public override int ReadEntityBody(byte[] buffer, int size)
    {
      return _context.Request.InputStream.Read(buffer, 0, size);
    }

    public override void SendKnownResponseHeader(int index, string value)
    {
      if (GetKnownRequestHeaderName(index) == "Content-Length")
      {
        _context.Response.ContentLength64 = long.Parse(value, CultureInfo.InvariantCulture);
        return;
      }
      try
      {
        _context.Response.Headers[
          GetKnownResponseHeaderName(index)] = value;
      }
      catch
      {
        Debug.WriteLine(string.Empty);
      }
    }

    public override void SendResponseFromFile(
      IntPtr handle, long offset, long length)
    {
      Debug.WriteLine(string.Empty);
    }

    public override void SendResponseFromFile(
      string filename, long offset, long length)
    {
      Debug.WriteLine(string.Empty);
    }

    public override void SendResponseFromMemory(byte[] data, int length)
    {
      _context.Response.OutputStream.Write(data, 0, length);
    }

    public override void SendStatus(int statusCode, string statusDescription)
    {
      _context.Response.StatusCode = statusCode;
      _context.Response.StatusDescription = statusDescription;
    }

    public override void SendUnknownResponseHeader(string name, string value)
    {
      _context.Response.Headers[name] = value;
    }
  }
}