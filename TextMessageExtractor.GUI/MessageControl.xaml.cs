﻿using System;
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
        public MessageControl(Message message, ContactDatabase contactDatabase)
        {
            InitializeComponent();
            this.Margin = new Thickness(0, 10, 0, 0);

            stackPanel.Margin = new Thickness(8);

            this.Background = message.incoming ? new SolidColorBrush(Color.FromRgb(0, 120, 215)) : new SolidColorBrush(Color.FromRgb(0, 48, 86));
            this.HorizontalAlignment = message.incoming ? HorizontalAlignment.Left : HorizontalAlignment.Right;

            if(message.msgType == Message.MessageType.SMS)
            {
                AddText(message.body);
            }
            else
            {
                if(message.body != "")
                {
                    //Add subject line
                    AddExtraText(message.body);
                }

                foreach (Message.Attachment attachment in message.attachments)
                {
                    if (attachment.contentType == "text/plain")
                    {
                        AddText(attachment.DataAsText);
                    }
                    else if (attachment.contentType.StartsWith("image/"))
                    {
                        AddImage(attachment.data);
                    }
                    else if (attachment.contentType != "application/smil")
                    {
                        stackPanel.Children.Add(new Label()
                        {
                            Height = 200,
                            Background = Brushes.DarkGray,
                            Content = $"Cannot display: Attachment of type{Environment.NewLine}{attachment.contentType}.",
                            Foreground = Brushes.White,
                            FontSize = 14,
                            HorizontalContentAlignment = HorizontalAlignment.Center,
                            VerticalContentAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(0, 2, 0, 2)
                        });
                    }
                }
            }
            //TODO: use Noda Time and allow user to adjust time zone
            DateTime dateTime = DateTime.FromFileTimeUtc(message.localTimestamp);
            AddExtraText(dateTime.ToString("MMM d, yyyy h:mm") + dateTime.ToString("%t").ToLower() + " UTC");
            if (message.incoming && message.Participants.Count > 1)
            {
                AddExtraText(contactDatabase.TryGetContactName(message.sender));
            }
        }

        new private void AddText(String text)
        {
            stackPanel.Children.Add(new TextBlock()
            {
                Text = text,
                Foreground = Brushes.White,
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 2, 0, 2)
            });
        }

        private void AddExtraText(String text)
        {
            stackPanel.Children.Add(new TextBlock()
            {
                Text = text,
                Foreground = new SolidColorBrush(Color.FromRgb(151, 201, 239)),
                TextWrapping = TextWrapping.Wrap,
                FontSize = 11,
            });
        }

        private void AddImage(byte[] imageData)
        {
            BitmapImage src = new BitmapImage();
            src.BeginInit();
            src.StreamSource = new MemoryStream(imageData);
            src.EndInit();

            Image im = new Image()
            {
                Source = src,
                Margin = new Thickness(0, 2, 0, 2)
            };
            stackPanel.Children.Add(im);
        }
    }
}
