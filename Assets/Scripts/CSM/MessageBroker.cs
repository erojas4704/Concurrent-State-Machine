using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CSM
{
    public class MessageBroker
    {
        /** Buffered messages for player input buffering. Messages that could not be processed to be processed in later frames. */
        private List<Message> messageBuffer = new List<Message>();

        /**Messages that are 'held,' such as held button inputs.*/
        private readonly Dictionary<string, Message> heldMessages = new Dictionary<string, Message>();

        /**New messages to be processed.*/
        private readonly List<Message> newMessages = new List<Message>();

        private HashSet<Message> messagesToProcessThisFrame;

        /**Bufferable messages that failed to be processed this frame.*/
        private readonly HashSet<Message> unprocessedMessages = new HashSet<Message>();

        [SerializeField] float bufferTime = 0.05f;


        public void EnqueueMessage(Message message)
        {
            if (message.name == "Move")
                Debug.Log($"Enqueued message {message} Phase: {message.phase}");
            newMessages.Add(message);
            if (message.phase == Message.Phase.Ended)
            {
                heldMessages.Remove(message.name);
            }
        }

        /**Must be called before ProcessMessagesForState. This prepares the list of messages to be sent to states.*/
        public void PrimeMessages()
        {
            messagesToProcessThisFrame = new HashSet<Message>(newMessages.Concat(messageBuffer));
        }

        public void CleanUp()
        {
            newMessages.Clear();
            //Add all messages to the messageBuffer that have not expired yet to try again next frame.
            messageBuffer.Clear();
            foreach (Message message in unprocessedMessages.Where(message => message.Timer < bufferTime))
                messageBuffer.Add(message);

            foreach (Message message in heldMessages.Values)
                message.phase = Message.Phase.Held;

            unprocessedMessages.Clear();
        }

        public void ProcessMessagesForState(State state)
        {
            //Process all buffered messages and in the queue.
            List<Message> messagesToProcessThisState = new List<Message>(messagesToProcessThisFrame);
            foreach (Message message in messagesToProcessThisState)
            {
                ProcessOrBufferMessage(state, message);
            }

            //Attempt to process all held messages
            foreach (Message message in heldMessages.Values)
            {
                state.Process(message);
            }
        }

        private void ProcessOrBufferMessage(State state, Message message)
        {
            bool blocking = state.Process(message);
            if (ShouldHoldMessage(message))
            {
                heldMessages[message.name] = message;
            }

            if (blocking)
            {
                messagesToProcessThisFrame.Remove(message);
            }

            if (message.processed)
            {
                unprocessedMessages.Remove(message);
            }
            else if (ShouldBufferMessage(message))
            {
                unprocessedMessages.Add(message);
            }
        }

        private bool ShouldHoldMessage(Message message) => message.phase == Message.Phase.Started && message.hold;

        private static bool ShouldBufferMessage(Message message) =>
            !message.processed && message.isBufferable && message.phase == Message.Phase.Started;
    }
}