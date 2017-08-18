﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextMessageExtractor
{
    [System.Diagnostics.DebuggerDisplay("{ToCommandLineString()}")]
    public class Message
    {
        public class Attachment
        {
            private static readonly Dictionary<String, String> Extensions = new Dictionary<String, String>()
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

            public String DataAsText => Encoding.Unicode.GetString(data);
            public bool IsText => contentType == "text/plain";
            public String DataFileExtension => Extensions[contentType];

            public override String ToString()
            {
                String dataString;
                if (IsText)
                {
                    dataString = DataAsText;
                }
                else
                {
                    dataString = $"<cannot represent as text>";
                }

                return $"{contentType}: {dataString}";
            }
        }

        public enum MessageType
        {
            SMS,
            MMS
        }

        public long localTimestamp;
        public String body;
        public MessageType msgType;
        public List<Attachment> attachments;

        public bool incoming;

        public List<String> recipients;
        public String sender;

        public HashSet<String> Participants
        {
            get
            {
                HashSet<String> participants = new HashSet<String>();
                if (recipients != null)
                    participants.UnionWith(recipients);
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
                    String extension = a.DataFileExtension;
                    File.WriteAllBytes(Path.Combine(folder, $"Attachment {i}.{extension}"), a.data);
                }
            }

            //Other information
            using (StreamWriter writer = new StreamWriter(Path.Combine(folder, "Info.txt")))
            {
                writer.WriteLine(incoming ? $"Incoming {msgType}" : $"Outgoing {msgType}");
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

        public String ToCommandLineString()
        {
            if(this.msgType == MessageType.SMS)
            {
                return body;
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                for(int i = 0; i < attachments.Count; i++)
                {
                    Attachment attachment = attachments[i];
                    sb.Append(attachment.IsText ? attachment.DataAsText : $"<{attachment.contentType} attachment>");
                    if (i != attachments.Count - 1)
                        sb.AppendLine();
                }
                return sb.ToString();
            }
        }
    }
}
