﻿using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange.JKrof.Interfaces
{
    public interface IRequest
    {
        Uri Uri { get; }
        WebHeaderCollection Headers { get; set; }
        string Method { get; set; }
        TimeSpan Timeout { get; set; }
        void SetProxy(string host, int port, string login, string password);

        string ContentType { get; set; }
        string Content { get; set; }
        string Accept { get; set; }
        long ContentLength { get; set; }

        Task<Stream> GetRequestStream();
        Task<IResponse> GetResponse();
    }
}
