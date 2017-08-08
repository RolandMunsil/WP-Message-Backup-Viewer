﻿using System;
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
        const String backupFolder = @"..\..\..\Backup copy\";

        static void Main(string[] args)
        {
            String currentPerson = null;
            Dictionary<String, String> phoneNumToName = new Dictionary<string, string>();
            foreach (String line in File.ReadAllLines(GetFileInFolder("contactsBackup", ".vcf")))
            {
                if (line.StartsWith("FN: "))
                {
                    currentPerson = line.Replace("FN: ", "").TrimEnd(' ');
                }
                else if (line.StartsWith("TEL;"))
                {
                    if (line.Contains("TYPE=CELL"))
                    {
                        int numStart = line.IndexOf("VOICE:") + "VOICE:".Length;
                        String phoneNum = FixStringIfNumber(line.Substring(numStart));
                        if (phoneNumToName.ContainsKey(phoneNum))
                        {
                            if (phoneNumToName[phoneNum] != currentPerson)
                            {
                                phoneNumToName[phoneNum] += $"/{currentPerson}";
                            }
                        }
                        else
                        {
                            phoneNumToName[phoneNum] = currentPerson;
                        }
                    }
                }
            }

            List<String> numbersForNamesWithMultipleNumbers = phoneNumToName
                                                              .GroupBy(kv => kv.Value)
                                                              .Where(g => g.Count() > 1)
                                                              .SelectMany(g => g)
                                                              .Select(kv=>kv.Key)
                                                              .ToList();

            foreach(String number in numbersForNamesWithMultipleNumbers)
            {
                phoneNumToName[number] += $" ({number})";
            }

            List<Message> smsMessages = ReadMessages(GetFileInFolder("smsBackup", ".msg"), Message.MessageType.SMS);
            List<Message> mmsMessages = ReadMessages(GetFileInFolder("mmsBackup", ".msg"), Message.MessageType.MMS);

            

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

            for(int i = 0; i < convos.Count; i++)
            {
                Console.WriteLine($"[{i}] {convos[i].ToString(phoneNumToName)}");
            }

            while (true)
            {
                Console.WriteLine();
                Console.Write("Enter conversation ID: ");
                int index = Int32.Parse(Console.ReadLine());
                foreach (Message m in convos[index].messages)
                {
                    String sender = m.incoming ? phoneNumToName[m.sender] : "Me";
                    Console.WriteLine($"{sender}: {m.ToCommandLineString()}");
                }
            }

            Console.ReadKey();
        }

        private static String GetFileInFolder(String folder, String fileExtension)
        {
            String fullFolderPath = Path.Combine(backupFolder, folder + @"\");

            foreach (String filename in Directory.EnumerateFiles(fullFolderPath))
            {
                if (filename.EndsWith(fileExtension))
                {
                    return filename;
                }
            }

            throw new FileNotFoundException();
        }

        private static List<Message> ReadMessages(String uri, Message.MessageType messageType)
        {
            XmlReader reader = XmlReader.Create(uri);

            while (reader.Name != "Message")
                reader.Read();

            List<Message> messages = new List<Message>();
            while (true)
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "ArrayOfMessage")
                    break;

                Message message = new Message();
                message.type = messageType;

                ErrorIfNodeNameIsNot(reader, "Message");
                reader.Read();

                //Recipients
                ErrorIfNodeNameIsNot(reader, "Recepients"); //Yes, it is spelled incorrectly
                if (!reader.IsEmptyElement)
                {
                    message.recipients = new List<String>();
                    reader.Read();
                    while (reader.Name == "string" && reader.NodeType == XmlNodeType.Element)
                    {
                        message.recipients.Add(FixStringIfNumber(reader.ReadElementContentAsString()));
                    }
                    ErrorIfNodeNameIsNot(reader, "Recepients");

                }
                reader.Read();

                //Body
                ErrorIfNodeNameIsNot(reader, "Body");
                message.body = reader.ReadElementContentAsString();

                //IsIncoming
                ErrorIfNodeNameIsNot(reader, "IsIncoming");
                message.incoming = reader.ReadElementContentAsBoolean();

                //IsRead (skip)
                ErrorIfNodeNameIsNot(reader, "IsRead");
                reader.ReadElementContentAsBoolean();

                //Attachments
                ErrorIfNodeNameIsNot(reader, "Attachments");
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
                ErrorIfNodeNameIsNot(reader, "LocalTimestamp");
                message.localTimestamp = reader.ReadElementContentAsLong();

                //Sender
                if (!reader.IsEmptyElement)
                {
                    message.sender = FixStringIfNumber(reader.ReadElementContentAsString());
                }
                else
                {
                    reader.Read();
                }

                //Exit message
                if (reader.NodeType != XmlNodeType.EndElement)
                    throw new Exception();
                ErrorIfNodeNameIsNot(reader, "Message");
                reader.Read();

                messages.Add(message);
            }

            return messages;
        }

        private static String FixStringIfNumber(String str)
        {
            if(!str.Any(c => Char.IsDigit(c)))
            {
                return str;
            }
            else if(str.Any(c => Char.IsLetter(c)))
            {
                return str;
            }
            else
            {
                String onlyNumbers = new String(str.Where(c => Char.IsDigit(c)).ToArray());
                if(onlyNumbers.Length > 10)
                {
                    int lengthOfExt = onlyNumbers.Length - 10;
                    String ext = onlyNumbers.Substring(0, lengthOfExt);
                    if(ext == "1")
                    {
                        String last10 = onlyNumbers.Substring(ext.Length, 10);
                        return last10;
                    }
                    else
                    {
                        return onlyNumbers;
                    }
                }
                return onlyNumbers;
            }
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

        private static void ErrorIfNodeNameIsNot(XmlReader reader, String name)
        {
            if (reader.Name != name)
                throw new Exception();
        }
    }
}
