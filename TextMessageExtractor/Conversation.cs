using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextMessageExtractor
{
    class Conversation : IEnumerable<Message>
    {
        private HashSet<String> participants;
        private List<Message> messages;

        public IEnumerable<String> Participants => participants;

        public Conversation(IEnumerable<String> participants)
        {
            this.participants = new HashSet<String>(participants);
            messages = new List<Message>();
        }

        public Conversation(IEnumerable<Message> unsortedMessages)
        {
            this.messages = new List<Message>();
            foreach(Message message in unsortedMessages)
            {
                Add(message);
            }
            this.participants = messages[0].Participants;
        }

        public bool MessageBelongs(Message message)
        {
            return this.participants.SetEquals(message.Participants);
        }

        public void Add(Message message)
        {
            int index = messages.FindLastIndex(m => m.localTimestamp < message.localTimestamp);
            messages.Insert(index + 1, message);
        }

        public override String ToString()
        {
            return $"{(String.Join(", ", participants))} ({messages.Count} messages)";
        }

        public IEnumerator<Message> GetEnumerator()
        {
            return ((IEnumerable<Message>)messages).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Message>)messages).GetEnumerator();
        }
    }
}
