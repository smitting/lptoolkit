using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.MIDI
{
    /// <summary>
    /// Events from a ScheduledMidiMessage.
    /// </summary>
    public delegate void ScheduledMidiMessageHandler(ScheduledMidiMessage msg);

    /// <summary>
    /// A message already scheduled to be sent via the MidiOutputThread.
    /// </summary>
    [Obsolete]
    public class ScheduledMidiMessage
    {
        #region Properties

        /// <summary>
        /// The driver that will be sending the message.
        /// </summary>
        public MidiDriver Driver;

        /// <summary>
        /// The outgoing message.
        /// </summary>
        public MidiMessage Message;

        /// <summary>
        /// When the message was inserted into the output thread.
        /// </summary>
        public DateTime ScheduledAt;

        /// <summary>
        /// When the message got sent by the output thread.
        /// </summary>
        public DateTime ProcessedAt { get; private set; }

        /// <summary>
        /// Tells the output thread that Cancel() was called.
        /// </summary>
        public bool Cancelled
        {
            get
            {
                lock (_cancelLock)
                {
                    return _cancelled;
                }
            }
        }

        /// <summary>
        /// True iff the message has been sent.
        /// </summary>
        public bool Processed
        {
            get { return _processed; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// If called before the scheduler sends the message, then it
        /// is cancelled returning true.  Returns false if the cancel
        /// didn't get sent in time.
        /// </summary>
        public bool Cancel()
        {
            lock (_cancelLock)
            {
                _cancelled = true;
                return !Processed;
            }
        }

        /// <summary>
        /// If this message is already processed, the callback is 
        /// immediately called, otherwise it is assigned to the Sent
        /// event.
        /// </summary>
        public void OnProcessed(ScheduledMidiMessageHandler callback)
        {
            lock (_processedLock)
            {
                if (_processed)
                {
                    callback(this);
                }
                else
                {
                    Sent += callback;
                }
            }
        }

        /// <summary>
        /// Sets the ProcessedAt time, marks the message as processed,
        /// and calls the Sent event.
        /// </summary>
        public void MarkProcessed()
        {
            ProcessedAt = DateTime.Now;
            lock (_processedLock)
            {
                _processed = true;
                if (Sent != null)
                {
                    Sent(this);
                }
            }
        }

        #endregion

        #region Private

        /// <summary>
        /// Gets set to true when the message is sent to the device.
        /// </summary>
        private bool _processed = false;

        /// <summary>
        /// Gets set to true when Cancel() is called.
        /// </summary>
        private bool _cancelled = false;

        /// <summary>
        /// Event called when the message is actually sent to the OS.
        /// This is private to force use of OnProcessed() and the
        /// protection is provides.
        /// </summary>
        private event ScheduledMidiMessageHandler Sent;

        /// <summary>
        /// Mutex to make sure there is no race condition when cancelling
        /// an event, so there is never a mismatch between reporting that
        /// a cancel happened and if it was actually cancelled.
        /// </summary>
        private readonly object _cancelLock = new object();

        /// <summary>
        /// Mutex to make sure no race condition exists for the Sent
        /// event when using OnProcessed();
        /// </summary>
        private readonly object _processedLock = new object();

        #endregion
    }

}
