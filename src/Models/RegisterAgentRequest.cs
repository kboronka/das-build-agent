using System;
using System.Collections.Generic;
using System.Text;

using HttpPack;

namespace DasBuildAgent.Models
{
    public class RegisterAgentRequest : IJsonObject
    {
        private readonly Agent agent;
        public RegisterAgentRequest(Agent agent)
        {
            this.agent = agent;
        }

        public JsonKeyValuePairs KeyValuePairs
        {
            get
            {
                var kvp = new JsonKeyValuePairs()
                {
                    {"name", agent.Name},
                    {"type", agent.Type},
                    {"port", agent.Port}
                };

                return kvp;
            }
        }
    }
}
