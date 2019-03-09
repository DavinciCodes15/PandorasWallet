﻿namespace Pandora.Client.Exchange.JKrof.Objects
{
    public enum RateLimitingBehaviour
    {
        Fail,
        Wait
    }

    public enum PostParameters
    {
        InBody,
        InUri
    }

    public enum RequestBodyFormat
    {
        FormData,
        Json
    }

    public enum SocketType
    {
        Normal,
        Background,
        BackgroundAuthenticated
    }
}
