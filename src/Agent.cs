using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using HttpPack.Server;
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

        public int Port { get; private set; }
        public string Secret { get; private set; }
        public string Sandbox { get; private set; }
        public string BuildServer { get; private set; }


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
                catch
                {
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
                    state = CheckAutomationServer();
                    break;
                case States.Unregistered:
                    state = Initialize();
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
            this.Port = int.Parse(Environment.GetEnvironmentVariable("PORT") ?? "4700");
            this.Secret = Environment.GetEnvironmentVariable("SECRET") ?? "secret-goes-here";
            this.Sandbox = Environment.GetEnvironmentVariable("SANDBOX_PATH") ?? @"c:\temp";

            server = new HttpServer(this.Port, null, this);

            return States.Idle;
        }

        private States CheckAutomationServer()
        {
            this.BuildServer = Environment.GetEnvironmentVariable("DAS_BUILD_SERVER") ?? @"http://localhost:4600";
            // TODO: check if automation server is on-line

            return States.Unregistered;
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
