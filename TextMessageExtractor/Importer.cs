using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TextMessageExtractor
{
    class Importer
    {
        String exportedBackupPath;
        Func<String, String> PhoneNumberNormalizer;

        public Importer(String exportedBackupPath, Func<String, String> PhoneNumberNormalizer, bool normalizePermanently)
        {
            this.exportedBackupPath = exportedBackupPath;
            this.PhoneNumberNormalizer = PhoneNumberNormalizer;
            if(normalizePermanently == false)
            {
                throw new NotImplementedException();
            }
        }

        public ContactDatabase ImportContacts()
        {          
            ContactDatabase contactDatabase = new ContactDatabase();

            String currentPerson = null;
            foreach (String line in File.ReadAllLines(GetFileInFolder("contactsBackup", ".vcf")))
            {
                if (line.StartsWith("FN: "))
                {
                    currentPerson = line.Replace("FN: ", "").TrimEnd(' ');
                }
                else if (line.StartsWith("TEL;") && line.Contains("TYPE=CELL"))
                {
                    int numStart = line.IndexOf("VOICE:") + "VOICE:".Length;
                    String phoneNum = TryNormalize(line.Substring(numStart));

                    contactDatabase.Add(phoneNum, currentPerson);
                }
            }

            return contactDatabase;
        }

        public MessageDatabase ImportMessages()
        {
            MessageDatabase messageDatabase = new MessageDatabase();

            ImportSingleMessageType(messageDatabase, GetFileInFolder("smsBackup", ".msg"), Message.MessageType.SMS);
            ImportSingleMessageType(messageDatabase, GetFileInFolder("mmsBackup", ".msg"), Message.MessageType.MMS);

            return messageDatabase;
        }

        private void ImportSingleMessageType(MessageDatabase messageDatabase, String fileUri, Message.MessageType messageType)
        {
            XmlReader reader = XmlReader.Create(fileUri);

            while (reader.Name != "Message")
                reader.Read();

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
                        message.recipients.Add(TryNormalize(reader.ReadElementContentAsString()));
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
                    message.sender = TryNormalize(reader.ReadElementContentAsString());
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

                messageDatabase.Add(message);
            }
        }

        private static void ErrorIfNodeNameIsNot(XmlReader reader, String name)
        {
            if (reader.Name != name)
                throw new Exception();
        }

        private String GetFileInFolder(String folder, String fileExtension)
        {
            String fullFolderPath = Path.Combine(exportedBackupPath, folder);

            return Directory.EnumerateFiles(fullFolderPath).Single(filename => filename.EndsWith(fileExtension));
        }

        private String TryNormalize(String str)
        {
            return IsPhoneNumber(str) ? PhoneNumberNormalizer(str) : str;
        }

        //NOTE: this is probably not the same algorithm as windows phone, just what seems reasonable to me.
        private bool IsPhoneNumber(String str)
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
    }
}
