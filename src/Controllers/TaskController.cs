using System;
using System.Collections.Generic;
using System.Text;

using HttpPack.Json;
using HttpPack.Server;
using DasBuildAgent.Models;

namespace DasBuildAgent.Controllers
{
    [IsPrimaryController]
    [IsController]
    class TaskController
    {
        [IsPrimaryAction]
        public static HttpContent Index(HttpRequest request)
        {
            var sample = new JsonKeyValuePairs
            {
                { "test", "ok" }
            };

            return new HttpJsonContent(sample);
        }

        public static HttpContent Start(HttpRequest request)
        {
            var agent = (Agent)request.UserData;
            var taskStartRequest = new TaskStartRequest(request, agent.Secret);
            agent.CommandQueue.QueueCommand(Agent.Commands.StartTask, taskStartRequest);

            var sample = new JsonKeyValuePairs
            {
                { "message", "task starting" }
            };

            return new HttpJsonContent(sample);
        }
    }
}
