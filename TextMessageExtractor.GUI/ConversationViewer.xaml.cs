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

        //int nextMessageToAdd = -1;
        //Conversation conversation;
        //bool dynamic = false;

        public ConversationViewer()
        {
            InitializeComponent();

            //scrollViewer.ScrollChanged += delegate (object sender, ScrollChangedEventArgs e)
            //{
            //    if(dynamic && nextMessageToAdd >= 0)
            //    {
            //        if(e.VerticalOffset < 640 && e.ExtentHeightChange == 0)
            //        {
            //            stackPanel.Children.Insert(0, new MessageControl(conversation.ElementAt(nextMessageToAdd--), ContactDatabase) { MaxWidth = 270 });
            //        }
            //        else if(e.VerticalOffset < 640 && e.ExtentHeightChange > 0)
            //        {
            //            scrollViewer.ScrollToVerticalOffset(e.ExtentHeightChange + e.VerticalOffset);
            //        }
            //    }
            //};
        }

        public void ViewConversation(Conversation conversation)
        {
            //stackPanel.Children.Clear();
            //stackPanel.UpdateLayout();
            //scrollViewer.ScrollToBottom();

            //dynamic = false;
            //nextMessageToAdd = conversation.Count() - 1;

            //this.conversation = conversation;

            //while (nextMessageToAdd >= 0 && stackPanel.ActualHeight <= scrollViewer.ActualHeight + 640)
            //{
            //    stackPanel.Children.Insert(0, new MessageControl(conversation.ElementAt(nextMessageToAdd--), ContactDatabase) { MaxWidth = 270 });
            //    stackPanel.UpdateLayout();
            //}
            //dynamic = true;


            stackPanel.Children.Clear();
            foreach (Message message in conversation)
            {
                stackPanel.Children.Add(new MessageControl(message, ContactDatabase) { MaxWidth = 270 });
            }
            scrollViewer.ScrollToBottom();
        }
    }
}
