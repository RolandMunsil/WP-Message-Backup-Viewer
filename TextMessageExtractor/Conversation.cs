using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextMessageExtractor
{
    class Conversation
    {
        class MessageComparer : IComparer<Message>
        {
            public int Compare(Message x, Message y)
            {
                return x.localTimestamp.CompareTo(y.localTimestamp);
            }
        }

        HashSet<String> participants;
        public SortedSet<Message> messages;

        public Conversation(IEnumerable<String> participants)
        {
            this.participants = new HashSet<String>(participants);
            messages = new SortedSet<Message>(new MessageComparer());
        }

        public bool ParticipantsMatch(IEnumerable<String> otherParticipants)
        {
            return this.participants.SetEquals(otherParticipants);
        }

        public void AddMessage(Message message)
        {
            messages.Add(message);
        }

        public override String ToString()
        {
            return String.Join(", ", participants);
        }
    }
}
