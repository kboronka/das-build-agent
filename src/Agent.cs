using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using HttpPack;
using HttpPack.Fsm;
using DasBuildAgent.Models;

namespace DasBuildAgent
{
    public class Agent : StateMachine
    {
        private HttpServer server;
        private States state;
        private Task task;

        public Agent()
        {

        }

        public string ID { get; private set; }
        public int Port { get; private set; }
        public string Secret { get; private set; }
        public string Sandbox { get; private set; }
        public string BuildServer { get; private set; }
        public string Name { get; private set; }
        public string Type { get; private set; }


        public override void Start()
        {
            loopStopped = false;
            loopStopRequested = false;
            state = States.Uninitialized;

            stateMachineThread = new Thread(Loop)
            {
                IsBackground = true,
                Name = "Agent",
                Priority = ThreadPriority.Lowest
            };

            stateMachineThread.Start();
        }

        public override void Stop()
        {
            loopStopRequested = true;
            if (stateMachineThread != null && stateMachineThread.IsAlive)
            {
                stateMachineThread.Join();
            }
        }

        private void Loop()
        {
            while (!loopStopped)
            {
                try
                {
                    ProcessStateMachine();
                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Thread.Sleep(5000);
                }
            }
        }

        private enum States
        {
            Uninitialized,
            Unregistered,
            Registered,
            Idle,

            StartTask,
            TaskRunning,
            TaskComplete,

            Stopping,
            Stopped
        }

        public enum Commands
        {
            StartTask
        }

        public void ProcessStateMachine()
        {
            switch (state)
            {
                case States.Uninitialized:
                    state = Initialize();
                    break;
                case States.Unregistered:
                    state = RegisterAgent();
                    break;
                case States.Idle:
                    state = ProcessCommands();
                    break;
                case States.StartTask:
                    // FIXME: extract into a method
                    task.Start();
                    state = States.TaskRunning;
                    break;
                case States.TaskRunning:
                    // TODO: not implemented yet
                    state = ProcessCommands();
                    break;
                case States.TaskComplete:
                    // TODO: not implemented yet
                    state = ProcessCommands();
                    break;
                case States.Stopping:
                    state = StopServer();
                    break;
                case States.Stopped:
                    base.loopStopped = true;
                    break;
            }
        }

        private States StopServer()
        {
            server.Stop();
            return States.Stopped;
        }

        private States Initialize()
        {
            ReadEnviormentVariables();
            StartHttpServer();
            return States.Unregistered;
        }

        private void StartHttpServer()
        {
            server = new HttpServer(this.Port, null, this);
        }

        private void ReadEnviormentVariables()
        {
            this.Port = int.Parse(Environment.GetEnvironmentVariable("PORT") ?? "4700");
            this.Name = Environment.GetEnvironmentVariable("AGENT_NAME") ?? @"undefined name";
            this.Type = Environment.GetEnvironmentVariable("AGENT_TYPE") ?? @"undefined type";
            this.Secret = Environment.GetEnvironmentVariable("SECRET") ?? "secret-goes-here";
            this.Sandbox = Environment.GetEnvironmentVariable("SANDBOX_PATH") ?? @"c:\temp\das-build-agent\";
            this.BuildServer = Environment.GetEnvironmentVariable("DAS_BUILD_SERVER") ?? @"http://localhost:4000";
        }

        private States RegisterAgent()
        {
            var uri = Url.Combine(this.BuildServer, "/agents/register");
            var req = new RegisterAgentRequest(this);
            var auth = GetJwtToken();

            var client = new HttpClient<JsonKeyValuePairs>();
            var res = client.Post(uri, req, auth);

            if (res.Code == 201)
            {
                var resAgent = (JsonKeyValuePairs)res.Body["agent"];
                this.ID = (string)resAgent["_id"];
                return States.Idle;
            }

            return States.Idle;
        }

        private string GetJwtToken()
        {
            var payload = new JsonKeyValuePairs()
            {
                {"sub", "das-build-agent"},
                {"name", this.Name},
                {"port", this.Port},
            };

            var jwt = new Jwt(payload, this.Secret);
            var auth = "JWT " + jwt.Token;
            return auth;
        }

        private States ProcessCommands()
        {
            if (loopStopRequested)
            {
                return States.Stopping;
            }
            else if (CommandQueue.Available)
            {
                var command = CommandQueue.DequeueCommand();
                switch ((Commands)command.CommandSignal)
                {
                    case Commands.StartTask:
                        var taskStartRequest = (TaskStartRequest)command.Parameters[0];
                        this.task = new Task(this, taskStartRequest);
                        return States.StartTask;
                }
            }

            return States.Idle;
        }
    }
}
