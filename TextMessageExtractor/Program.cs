using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TextMessageExtractor
{
    class Program
    {
        static String smsFile = @"C:\Users\rolan\Desktop\Text Message Backups\smsBackup\Mon, Apr 17 2017, 21-27-39 PM.msg";
        static String mmsFile = @"C:\Users\rolan\Desktop\Text Message Backups\mmsBackup\Mon, Apr 17 2017, 21-38-52 PM.msg";
        //static String mmsFile = @"mmsNice.xml";

        static XmlReader reader;
        //static List<Message> messages;

        static void Main(string[] args)
        {
            List<Message> mmsMessages = ReadMessages(mmsFile, Message.MessageType.MMS);
            List<Message> smsMessages = ReadMessages(smsFile, Message.MessageType.SMS);

            Console.WriteLine("Done reading messages");
            List<Message> allMessages = mmsMessages.Concat(smsMessages).ToList();

            //Group by conversation
            List<Conversation> convos = new List<Conversation>();

            foreach (Message message in allMessages)
            {
                List<String> participants = message.Participants;

                bool matchFound = false;
                foreach (Conversation convo in convos)
                {
                    if (convo.ParticipantsMatch(participants))
                    {
                        convo.AddMessage(message);
                        matchFound = true;
                        break;
                    }
                }
                if (matchFound)
                {
                    continue;
                }
                else
                {
                    Conversation newConvo = new Conversation(participants);
                    newConvo.AddMessage(message);
                    convos.Add(newConvo);
                }
            }

            //foreach (Message m in mmsMessages.Concat(smsMessages))
            //{
            //    m.SaveToFolder("Messages/" + m.localTimestamp.ToString());
            //}
            //Console.WriteLine("Done making folders");

            //Console.WriteLine("Available numbers");
            //WriteEnumerable(messages.Where(m => m.recipients != null).SelectMany(m => m.recipients).Distinct());

            ////foreach(SMSMessage m in messages.Where(m => m.recipients != null && m.recipients.Count > 1))
            ////{
            ////    Console.WriteLine(m.body);
            ////}
            //while (true)
            //{
            //    Console.WriteLine();
            //    Console.Write("Who would you like to see conversation history with? ");
            //    String number = Console.ReadLine();
            //    WriteEnumerable(from message in messages
            //                    where message.sender == number || (!message.incoming && message.recipients.Contains(number))
            //                    orderby message.localTimestamp
            //                    select $"{(message.incoming ? "Them" : "Me")}: {message.body}"
            //        );
            //}

            Console.ReadKey();
        }

        private static List<Message> ReadMessages(String uri, Message.MessageType messageType)
        {
            reader = XmlReader.Create(uri);

            while (reader.Name != "Message")
                reader.Read();

            List<Message> messages = new List<Message>();
            while (true)
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "ArrayOfMessage")
                    break;

                Message message = new Message();
                message.type = messageType;

                ErrorIfNodeNameIsNot("Message");
                reader.Read();

                //Recipients
                ErrorIfNodeNameIsNot("Recepients"); //Yes, it is spelled incorrectly
                if (!reader.IsEmptyElement)
                {
                    message.recipients = new List<string>();
                    reader.Read();
                    while (reader.Name == "string" && reader.NodeType == XmlNodeType.Element)
                    {
                        message.recipients.Add(reader.ReadElementContentAsString());
                    }
                    ErrorIfNodeNameIsNot("Recepients");

                }
                reader.Read();

                //Body
                ErrorIfNodeNameIsNot("Body");
                message.body = reader.ReadElementContentAsString();

                //IsIncoming
                ErrorIfNodeNameIsNot("IsIncoming");
                message.incoming = reader.ReadElementContentAsBoolean();

                //IsRead (skip)
                ErrorIfNodeNameIsNot("IsRead");
                reader.ReadElementContentAsBoolean();

                //Attachments
                ErrorIfNodeNameIsNot("Attachments");
                if (!reader.IsEmptyElement)
                {
                    message.attachments = new List<Message.Attachment>();
                    reader.Read();
                    while (reader.Name == "MessageAttachment" && reader.NodeType == XmlNodeType.Element)
                    {
                        reader.Read();
                        Message.Attachment a = new Message.Attachment();
                        a.contentType = reader.ReadElementContentAsString();

                        char[] base64 = reader.ReadElementContentAsString().ToCharArray();
                        a.data = Convert.FromBase64CharArray(base64, 0, base64.Length);

                        message.attachments.Add(a);

                        reader.Read();
                    }
                    //reader.Read();
                }
                reader.Read();

                //LocalTimestamp
                ErrorIfNodeNameIsNot("LocalTimestamp");
                message.localTimestamp = reader.ReadElementContentAsLong();

                //Sender
                if (!reader.IsEmptyElement)
                {
                    message.sender = reader.ReadElementContentAsString();
                }
                else
                {
                    reader.Read();
                }

                //Exit message
                if (reader.NodeType != XmlNodeType.EndElement)
                    throw new Exception();
                ErrorIfNodeNameIsNot("Message");
                reader.Read();

                messages.Add(message);
            }

            return messages;
        }

        private static void PrettyPrintXML(String src, String dest)
        {
            XmlWriter.Create(dest, new XmlWriterSettings { Indent = true }).WriteNode(XmlReader.Create(src), true);
        }

        private static void WriteEnumerable<T>(IEnumerable<T> e)
        {
            foreach (T o in e)
            {
                Console.WriteLine(o);
            }
        }

        private static void ErrorIfNodeNameIsNot(String name)
        {
            if (reader.Name != name)
                throw new Exception();
        }
    }
}
