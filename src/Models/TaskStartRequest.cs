﻿using System;
using System.Collections.Generic;
using System.Text;

using HttpPack;

namespace DasBuildAgent.Models
{
    public class TaskStartRequest
    {
        public TaskStartRequest(HttpRequest request, string secret)
        {
            // TODO: not implemented yet
            var authorization = request.Header;
            var remoteEndpoint = request.RemoteEndpoint;
        }
    }
}
