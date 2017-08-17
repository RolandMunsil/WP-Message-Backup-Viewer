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
using System.Windows.Shapes;

namespace TextMessageExtractor.GUI
{
    /// <summary>
    /// Interaction logic for Message.xaml
    /// </summary>
    public partial class MessageControl : UserControl
    {
        public MessageControl(Message message)
        {
            InitializeComponent();

            this.Background = message.incoming ? Brushes.LightBlue : Brushes.DarkBlue;
            this.HorizontalAlignment = message.incoming ? HorizontalAlignment.Left : HorizontalAlignment.Right;

            if(message.msgType == Message.MessageType.SMS)
            {
                stackPanel.Children.Add(new TextBlock()
                {
                    Text = message.body,
                    Foreground = Brushes.White,
                    TextWrapping = TextWrapping.Wrap
                });
            }
            else
            {
                foreach (Message.Attachment attachment in message.attachments)
                {
                    if (attachment.contentType == "text/plain")
                    {
                        stackPanel.Children.Add(new TextBlock()
                        {
                            Text = attachment.DataAsText,
                            Foreground = Brushes.White,
                            TextWrapping = TextWrapping.Wrap
                        });
                    }
                    else if (attachment.contentType.StartsWith("image/"))
                    {
                        BitmapImage src = new BitmapImage()
                        {
                            StreamSource = new MemoryStream(attachment.data)
                        };

                        stackPanel.Children.Add(new Image()
                        {
                            Source = src
                        });
                    }
                }
            }
        }
    }
}
