using System;
using System.Collections.Generic;
using System.Text;

using HttpPack.Json;
using HttpPack.Server;

namespace DasBuildAgent.Controllers
{
    [IsPrimaryController]
    [IsController]
    class TaskController
    {
        [IsPrimaryAction]
        public static HttpContent Index(HttpRequest request)
        {
            var sample = new JsonKeyValuePairs();
            sample.Add("test", "ok");

            return new HttpJsonContent(sample);
        }
    }
}
