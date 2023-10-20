using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CSM
{
    public class MessageBroker
    {
        /** Buffered messages for player input buffering. Messages that could not be processed to be processed in later frames. */
        private readonly List<Message> messageBuffer = new List<Message>();

        /**Messages that are 'held,' such as held button inputs.*/
        private readonly Dictionary<string, Message> heldMessages = new Dictionary<string, Message>();

        /**New messages to be processed.*/
        private readonly List<Message> newMessages = new List<Message>();

        /**Messages to be processed the entire frame across all states.*/
        private readonly HashSet<Message> messagesToProcessThisFrame = new HashSet<Message>();

        /**Bufferable messages that failed to be processed this frame.*/
        private readonly HashSet<Message> unprocessedMessages = new HashSet<Message>();

        private readonly HashSet<Message> blockedMessagesThisFrame = new HashSet<Message>();


        public void EnqueueMessage(Message message)
        {
            newMessages.Add(message);
            if (message.phase == Message.Phase.Ended)
            {
                heldMessages.Remove(message.name);
            }
        }

        /**Must be called before ProcessMessagesForState. This prepares the list of messages to be sent to states.*/
        public void PrimeMessages()
        {
            messagesToProcessThisFrame.UnionWith(newMessages.Concat(messageBuffer));
        }

        public void CleanUp(float bufferTime)
        {
            foreach (Message message in newMessages.Where(message => message.phase == Message.Phase.Ended))
            {
                heldMessages.Remove(message.name);
            }

            newMessages.Clear();
            //Add all messages to the messageBuffer that have not expired yet to try again next frame.
            messageBuffer.Clear();
            foreach (Message message in unprocessedMessages.Where(message => message.Timer < bufferTime))
                messageBuffer.Add(message);

            foreach (Message message in heldMessages.Values)
                message.phase = Message.Phase.Held;

            unprocessedMessages.Clear();
            blockedMessagesThisFrame.Clear();
            messagesToProcessThisFrame.Clear();
        }

        public void ProcessMessagesForState(State state)
        {
            //Process all buffered messages and in the queue.
            List<Message> messagesToProcessThisState = new List<Message>(messagesToProcessThisFrame);

            //Attempt to process all held messages. Held messages are processed first so 
            //messages that have just started don't become held and are processed twice.
            foreach (Message message in heldMessages.Values)
            {
                //Held messages cannot be blocked.
                if (!blockedMessagesThisFrame.Contains(message))
                    state.Process(message);
            }

            foreach (Message message in messagesToProcessThisState)
            {
                ProcessOrBufferMessage(state, message);
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
                blockedMessagesThisFrame.Add(message);
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

        private bool ShouldHoldMessage(Message message) => message.phase == Message.Phase.Started ||
                                                           message.phase == Message.Phase.Held && message.hold;

        private static bool ShouldBufferMessage(Message message) =>
            !message.processed && message.isBufferable && message.phase == Message.Phase.Started;

        internal bool ProcessMessagesForGhostState(Actor.GhostState ghost)
        {
            bool processed = false;
            //Ghost states do not get to block messages
            foreach (Message message in messagesToProcessThisFrame)
            {
                if (ghost.messagesToListenFor.Count > 0 && !ghost.messagesToListenFor.Contains(message.name))
                    continue;

                ghost.state.Process(message);
                if (message.processed)
                    processed = true;
            }

            return processed;
        }
    }
}