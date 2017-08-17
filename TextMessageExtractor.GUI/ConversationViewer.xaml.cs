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

using TextMessageExtractor;

namespace TextMessageExtractor.GUI
{
    /// <summary>
    /// Interaction logic for MessageViewer.xaml
    /// </summary>
    public partial class ConversationViewer : UserControl
    {
        public ContactDatabase ContactDatabase { get; set; }

        public ConversationViewer()
        {
            InitializeComponent(); 
        }

        public void ViewConversation(Conversation conversation)
        {
            stackPanel.Children.Clear();
            foreach (Message message in conversation)
            {
                stackPanel.Children.Add(new MessageControl(message, ContactDatabase) { MaxWidth = 270 });
            }
            scrollViewer.ScrollToBottom();
        }
    }
}
