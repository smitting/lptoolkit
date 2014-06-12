using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LPToolKit.Logs;
using LPToolKit.Util;
using LPToolKit.Core.Tasks;

namespace LPToolKit.Session.Managers
{  
    /// <summary>
    /// Manages standard output from the system and implants for 
    /// delivery to be displayed by the host application in an
    /// organize manner.
    /// </summary>
    public class ConsoleManager : SessionManagerBase
    {
        #region Constructors

        public ConsoleManager(UserSession parent) : base(parent)
        {
            _task = new ConsoleTask(this);
        }

        #endregion

        #region Properties

        /// <summary>
        /// All messages currently stored, listed in order received.
        /// </summary>
        public readonly OrdinalList<ConsoleMessage> Messages = new OrdinalList<ConsoleMessage>();

        /// <summary>
        /// Event called when new messages are added.
        /// </summary>
        public event NewConsoleMessageEventHandler MessageAdded;

        #endregion

        #region Methods

        /// <summary>
        /// Adds a message to the list, with all processing happening
        /// on a background thread.
        /// </summary>
        public void Add(string message, string source = null)
        {
            _consoleQueue.Enqueue(new ConsoleMessage() { Message = message, Source = source });
        }

        /// <summary>
        /// Returns all messages at or above a certain ordinal, 
        /// optionally from a specific source.
        /// </summary>
        public List<ConsoleMessage> GetSinceOrdinal(int ordinal, string source = null)
        {
            if (source == null)
            {
                return Messages.GetSinceOrdinal(ordinal);
            }
            else 
            {
                return Messages.GetSinceOrdinal(ordinal).Where(m => m.Source == source).ToList();
            }
        }

        #endregion

        #region Private

        /// <summary>
        /// Queue to prevent Add() from blocking.
        /// </summary>
        private OutputQueue<ConsoleMessage> _consoleQueue = new OutputQueue<ConsoleMessage>();

        /// <summary>
        /// Instance of the console task.
        /// </summary>
        private ConsoleTask _task;

        /// <summary>
        /// The task that repeatedly builds the queue from the insertions.
        /// </summary>
        private class ConsoleTask : RepeatingKernelTask
        {
            public ConsoleTask(ConsoleManager parent) : base()
            {
                Parent = parent;
                MinimumRepeatTimeMsec = 100;
                ExpectedLatencyMsec = 1000;
            }

            public readonly ConsoleManager Parent;

            public override void RunTask()
            {
                ConsoleMessage next;
                for (var i = 0; i < 10; i++)
                {
                    if (Parent._consoleQueue.Dequeue(out next, false))
                    {
                        Parent.Messages.Add(next);
                        if (Parent.MessageAdded != null)
                        {
                            Parent.MessageAdded(this, new NewConsoleMessageEventArgs() { Message = next });
                        }
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }

        #endregion

    }
}
