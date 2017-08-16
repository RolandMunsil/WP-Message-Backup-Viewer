using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextMessageExtractor
{
    class MessageDatabase
    {
        private List<Message> messages;

        public MessageDatabase()
        {
            this.messages = new List<Message>();
        }

        public void Add(Message message)
        {
            messages.Add(message);
        }

        public List<Conversation> GetConversations()
        {
            return messages
                  .GroupBy(m => m.Participants, HashSet<String>.CreateSetComparer())
                  .Select(g => new Conversation(g))
                  .ToList();
        }

    }
}
