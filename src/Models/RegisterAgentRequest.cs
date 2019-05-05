using System;
using System.Collections.Generic;
using System.Text;

using HttpPack;

namespace DasBuildAgent.Models
{
    public class RegisterAgentRequest : IJsonObject
    {
        public RegisterAgentRequest()
        {

        }

        public JsonKeyValuePairs KeyValuePairs
        {
            get
            {
                var kvp = new JsonKeyValuePairs();
                // TODO: implement

                return kvp;
            }
        }
    }
}
