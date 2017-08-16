using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using TextMessageExtractor;

namespace TextMessageExtractor.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            String backupFolder = String.Join(" ", args).Trim('"');

            Importer importer = new Importer(backupFolder, PhoneNumberNormalizers.UnitedStates);

            ContactDatabase contactDB = importer.ImportContacts();
            Console.WriteLine("Finished reading contacts");

            MessageDatabase messageDB = importer.ImportMessages();
            Console.WriteLine("Finished reading messages");

            List<Conversation> convos = messageDB.GetConversations().OrderByDescending(c=>c.MostRecentMessageTime).ToList();

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

        private static void PrettyPrintXML(String src, String dest)
        {
            XmlWriter.Create(dest, new XmlWriterSettings { Indent = true }).WriteNode(XmlReader.Create(src), true);
        }
    }
}
