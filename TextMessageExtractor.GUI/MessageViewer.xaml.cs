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
    public partial class MessageViewer : UserControl
    {
        public MessageViewer()
        {
            InitializeComponent();

            Importer importer = new Importer(@"C:\Users\rolan\OneDrive\Code\VS2017 Projects\TextMessageExtractor\TextMessageExtractor.GUI\bin\Debug\Backup copy", PhoneNumberNormalizers.UnitedStates);
            MessageDatabase messageDB = importer.ImportMessages();
            Conversation conversation = messageDB.GetConversations()[0];

            foreach(Message message in conversation)
            {
                stackPanel.Children.Add(new MessageControl(message) { Width = this.Width * 3.0 / 4.0 });
            }
        }
    }
}
