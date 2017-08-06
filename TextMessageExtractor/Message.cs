using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextMessageExtractor
{
    public class Message
    {
        public enum MessageType
        {
            SMS,
            MMS
        }

        public class Attachment
        {
            public static readonly Dictionary<String, String> Extensions = new Dictionary<String, String>()
            {
                { "text/plain", "txt" },
                { "text/x-vcard", "vcard" },
                { "application/smil", "smil" },
                { "image/png", "png" },
                { "image/jpeg", "jpg" },
                { "image/gif", "gif" },
                { "video/3gpp", "3gp" },
                { "video/mp4", "mp4" }
            };

            public String contentType;
            public byte[] data;

            public override String ToString()
            {
                String dataString;

                if (contentType == "text/plain" || contentType == "application/smil")
                {
                    dataString = Encoding.Unicode.GetString(data);
                }
                else
                {
                    dataString = "<not visualizable>";
                }

                return $"{contentType}: {dataString}";
            }
        }

        public long localTimestamp;
        public String body;
        public MessageType type;
        public List<Attachment> attachments;

        public bool incoming;

        public List<String> recipients;
        public String sender;

        public List<String> Participants
        {
            get
            {
                List<String> participants = new List<string>();
                if (recipients != null)
                    participants.AddRange(recipients);
                if (sender != null)
                    participants.Add(sender);
                return participants;
            }
        }

        public void SaveToFolder(String folder)
        {
            DirectoryInfo dir = Directory.CreateDirectory(folder);

            //Attachments
            if (attachments != null)
            {
                for (int i = 0; i < attachments.Count; i++)
                {
                    Message.Attachment a = attachments[i];
                    String extension = Attachment.Extensions[a.contentType];
                    File.WriteAllBytes(Path.Combine(folder, $"Attachment {i}.{extension}"), a.data);
                }
            }

            //Other information
            using (StreamWriter writer = new StreamWriter(Path.Combine(folder, "Info.txt")))
            {
                writer.WriteLine(incoming ? $"Incoming {type}" : $"Outgoing {type}");
                writer.WriteLine("APPROX TIME: " + DateTime.FromFileTimeUtc(localTimestamp).ToString("r"));
                if (sender != null)
                {
                    writer.WriteLine($"Sender: {sender}");
                }
                if (recipients != null)
                {
                    writer.WriteLine($"Recipients: {String.Join(", ", recipients)}");
                }
                writer.WriteLine("===Message body===");
                writer.Write(body);                
            }
        }
    }
}
