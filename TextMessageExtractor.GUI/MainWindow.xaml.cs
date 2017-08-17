using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace TextMessageExtractor.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Importer importer = new Importer("Backup copy", PhoneNumberNormalizers.UnitedStates);
            ContactDatabase contactDB = importer.ImportContacts();
            conversationViewer.ContactDatabase = contactDB;

            MessageDatabase messageDB = importer.ImportMessages();
            List<Conversation> conversations = messageDB.GetConversations();

            Dictionary<String, Conversation> displayStrToConversation = new Dictionary<string, Conversation>();

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
    }
}
