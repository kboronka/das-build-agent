using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using HttpPack;
using HttpPack.Fsm;
using DasBuildAgent.Models;

namespace DasBuildAgent
{
    public class Task : StateMachine
    {
        private readonly Agent agent;
        private readonly TaskStartRequest request;
        private States state;

        public Task(Agent agent, TaskStartRequest request)
        {
            this.agent = agent;
            this.request = request;

        }


        public override void Start()
        {
            loopStopped = false;
            loopStopRequested = false;
            state = States.ReadSequence;

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
            ReadSequence,
            CreateSandbox,
            ReadNextStep,
            RunProcess,
            SequenceComplete,
            SequencePassed,
            SequenceFailed,
            KillProcesses,
            DeleteSandbox,

            Stopping,
            Stopped
        }

        public void ProcessStateMachine()
        {
            switch (state)
            {
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
            return States.Stopped;
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
                // TODO: add commands
            }

            return States.ReadSequence;
        }
    }
}
