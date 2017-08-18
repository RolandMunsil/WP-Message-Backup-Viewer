using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace TextMessageExtractor.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Dictionary<String, Conversation> displayStrToConversation;
        ContactDatabase contactDB;

        public MainWindow()
        {
            InitializeComponent();

            Importer importer = new Importer("Backup copy", PhoneNumberNormalizers.UnitedStates);
            contactDB = importer.ImportContacts();
            conversationViewer.ContactDatabase = contactDB;

            MessageDatabase messageDB = importer.ImportMessages();
            List<Conversation> conversations = messageDB.GetConversations();

            displayStrToConversation = new Dictionary<string, Conversation>();

            foreach (Conversation conversation in conversations.OrderByDescending(c=>c.MostRecentMessageTime))
            {
                String displayStr = String.Join(", ", conversation.Participants.Select(s => contactDB.TryGetContactName(s)));
                displayStrToConversation[displayStr] = conversation;
                convoListBox.Items.Add(displayStr);
            }

            convoListBox.SelectionChanged += delegate (object sender, SelectionChangedEventArgs e)
            {
                conversationViewer.ViewConversation(displayStrToConversation[(String)e.AddedItems[0]]);
            };
        }

        private void exportImagesButton_Click(object sender, RoutedEventArgs e)
        {
            Conversation convo = displayStrToConversation[(String)convoListBox.SelectedItem];

            string participantsStr = (String)convoListBox.SelectedItem;

            foreach (char c in Path.GetInvalidPathChars())
            {
                participantsStr = participantsStr.Replace(c, '_');
            }

            DirectoryInfo dirInfo = Directory.CreateDirectory($"Images from conv with {participantsStr}");

            int messageID = 1;
            int maxDigits = convo.Count().ToString().Length;
            String fmtString = "d" + maxDigits;
            foreach(Message message in convo)
            {
                String msgSender = message.sender == null ? "Me" : contactDB.TryGetContactName(message.sender);

                foreach(char c in Path.GetInvalidFileNameChars())
                {
                    msgSender = msgSender.Replace(c, '_');
                }
                
                String dateAndTime = DateTime.FromFileTimeUtc(message.localTimestamp).ToString("yyyy-MM-dd HHmmss");
                String messageString = $"Msg {messageID.ToString(fmtString)} from {msgSender} at {dateAndTime}";
                String pathStart = Path.Combine(dirInfo.FullName, messageString);
                if (message.msgType == Message.MessageType.MMS)
                {
                    List<Message.Attachment> imageAttachments = message.attachments.Where(a => a.contentType.StartsWith("image/")).ToList();
                    if(imageAttachments.Count == 0)
                    {
                        continue;
                    }
                    else if(imageAttachments.Count == 1)
                    {
                        File.WriteAllBytes($"{pathStart}.{imageAttachments[0].DataFileExtension}", imageAttachments[0].data);
                    }
                    else
                    {
                        for(int i = 0; i < imageAttachments.Count; i++)
                        {
                            File.WriteAllBytes($"{pathStart} {i}.{imageAttachments[i].DataFileExtension}", imageAttachments[i].data);
                        }
                    }
                }

                messageID++;
            }
        }
    }
}
