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
        static void Main(string[] args)
        {
            String backupFolder = String.Join(" ", args).Trim('"');

            ContactDatabase contactDB = CreateNumberToNameMap(backupFolder, NormalizeNumberUnitedStates);
            Console.WriteLine("Finished reading contacts");

            List<Message> allMessages = ReadAllMessages(backupFolder, NormalizeNumberUnitedStates);
            Console.WriteLine("Finished reading messages");

            List<Conversation> convos = GroupIntoConversations(allMessages);

            for (int i = 0; i < convos.Count; i++)
            {
                String participantList = String.Join(", ", convos[i].Participants.Select(s => contactDB.TryGetContactName(s)));
                Console.WriteLine($"[{i}] {participantList}");
            }

            while (true)
            {
                Console.WriteLine();
                Console.Write("Enter conversation ID: ");
                int index = Int32.Parse(Console.ReadLine());
                foreach (Message m in convos[index])
                {
                    String sender;
                    if (!m.incoming)
                    {
                        sender = "Me";
                    }
                    else
                    {
                        //Use their name from the user's contacts
                        sender = contactDB.TryGetContactName(m.sender);
                    }
                    Console.WriteLine($"{sender}: {m.ToCommandLineString()}");
                }
            }

            Console.ReadKey();
        }

        private static List<Conversation> GroupIntoConversations(List<Message> allMessages)
        {
            return allMessages
                   .GroupBy(m => m.Participants, HashSet<String>.CreateSetComparer())
                   .Select(g => new Conversation(g))
                   .ToList();
        }

        private static List<Message> ReadAllMessages(string backupFolder, Func<String, String> phoneNumNormalizer = null)
        {
            List<Message> smsMessages = ReadMessages(GetFileInFolder(backupFolder, "smsBackup", ".msg"), Message.MessageType.SMS);
            List<Message> mmsMessages = ReadMessages(GetFileInFolder(backupFolder, "mmsBackup", ".msg"), Message.MessageType.MMS);
            List<Message> allMessages = mmsMessages.Concat(smsMessages).ToList();

            if(phoneNumNormalizer != null)
            {
                foreach(Message message in allMessages)
                {
                    if(message.sender != null)
                    {
                        message.sender = NormalizeIfNumber(message.sender, phoneNumNormalizer);
                    }
                    if(message.recipients != null)
                    {
                        for(int i = 0; i < message.recipients.Count; i++)
                        {
                            message.recipients[i] = NormalizeIfNumber(message.recipients[i], phoneNumNormalizer);
                        }
                    }
                }
            }
            return allMessages;
        }

        private static ContactDatabase CreateNumberToNameMap(string backupFolder, Func<String, String> phoneNumNormalizer)
        {
            String currentPerson = null;
            Dictionary<String, String> phoneNumToName = new Dictionary<string, string>();
            foreach (String line in File.ReadAllLines(GetFileInFolder(backupFolder, "contactsBackup", ".vcf")))
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
                        String phoneNum = NormalizeIfNumber(line.Substring(numStart), phoneNumNormalizer);
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
                                                              .Select(kv => kv.Key)
                                                              .ToList();

            foreach (String number in numbersForNamesWithMultipleNumbers)
            {
                phoneNumToName[number] += $" ({number})";
            }

            return new ContactDatabase(phoneNumToName);
        }

        private static String GetFileInFolder(String backupFolder, String folder, String fileExtension)
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
                message.msgType = messageType;

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
                        message.recipients.Add(reader.ReadElementContentAsString());
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
                    message.sender = reader.ReadElementContentAsString();
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
        
        private static String NormalizeIfNumber(String str, Func<String, String> normalizationFunc)
        {
            return IsPhoneNumber(str) ? normalizationFunc(str) : str;
        }

        private static String NormalizeNumberUnitedStates(String str)
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

        //NOTE: this is probably not the same algorithm as windows phone, just what seems reasonable to me.
        private static bool IsPhoneNumber(String str)
        {
            if (!str.Any(c => Char.IsDigit(c)))
            {
                return false;
            }
            else if (str.Any(c => Char.IsLetter(c)))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private static void PrettyPrintXML(String src, String dest)
        {
            XmlWriter.Create(dest, new XmlWriterSettings { Indent = true }).WriteNode(XmlReader.Create(src), true);
        }

        private static void ErrorIfNodeNameIsNot(XmlReader reader, String name)
        {
            if (reader.Name != name)
                throw new Exception();
        }
    }
}
