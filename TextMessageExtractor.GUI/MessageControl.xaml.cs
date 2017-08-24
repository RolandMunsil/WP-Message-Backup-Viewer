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
using System.Xml;

namespace TextMessageExtractor.GUI
{
    /// <summary>
    /// Interaction logic for Message.xaml
    /// </summary>
    public partial class MessageControl : UserControl
    {
        private Message message;

        public MessageControl(Message message, ContactDatabase contactDatabase)
        {
            InitializeComponent();
            this.message = message;

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

                //foreach (Message.Attachment attachment in message.attachments)
                for(int i = 0; i < message.attachments.Count; i++)
                {
                    Message.Attachment attachment = message.attachments[i];
                    if (attachment.IsText)
                    {
                        AddText(attachment.DataAsText);
                    }
                    else if (attachment.IsImage)
                    {
                        AddImage(attachment);
                    }
                    else if (!attachment.IsSMIL)
                    {
                        Label attachmentLabel = new Label()
                        {
                            Height = 200,
                            Background = Brushes.DarkGray,
                            Content = $"Cannot display: Attachment of type{Environment.NewLine}{attachment.contentType}.",
                            Foreground = Brushes.White,
                            FontSize = 14,
                            HorizontalContentAlignment = HorizontalAlignment.Center,
                            VerticalContentAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(0, 2, 0, 2)
                        };

                        ContextMenu labelContextMenu = new ContextMenu();
                        labelContextMenu.Items.Add(MakeMenuItem("Save Attachment As...", attachment, SaveAttachment_Click));
                        attachmentLabel.ContextMenu = labelContextMenu;

                        stackPanel.Children.Add(attachmentLabel);

                        
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

        private MenuItem MakeMenuItem(String text, object attachedObj, RoutedEventHandler ClickEventHandler)
        {
            MenuItem menuItem = new MenuItem()
            {
                Header = text,
                Tag = attachedObj
            };
            menuItem.Click += ClickEventHandler;
            return menuItem;
        }

        new private void AddText(String text)
        {
            TextBlock textBlock = new TextBlock()
            {
                Text = text,
                Foreground = Brushes.White,
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 2, 0, 2),

                //IsReadOnly = true,
                //IsReadOnlyCaretVisible = false,

                //Background = Brushes.Transparent,
                //BorderBrush = Brushes.Transparent,
                //SelectionBrush = Brushes.Black,
                //BorderThickness = new Thickness(0),
            };

            ContextMenu textContextMenu = new ContextMenu();
            textContextMenu.Items.Add(MakeMenuItem("Copy Text", text, CopyText_Click));
            textBlock.ContextMenu = textContextMenu;

            stackPanel.Children.Add(textBlock);
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

        private void AddImage(Message.Attachment imageAttachment)
        {
            BitmapImage src = new BitmapImage();
            src.BeginInit();
            src.StreamSource = new MemoryStream(imageAttachment.data);
            src.EndInit();

            Image im = new Image()
            {
                Source = src,
                Margin = new Thickness(0, 2, 0, 2)
            };

            ContextMenu imageContextMenu = new ContextMenu();
            imageContextMenu.Items.Add(MakeMenuItem("Copy Image", src, CopyImage_Click));
            imageContextMenu.Items.Add(MakeMenuItem("Save Image As...", imageAttachment, SaveAttachment_Click));
            im.ContextMenu = imageContextMenu;

            stackPanel.Children.Add(im);
        }

        private void SaveAttachment(Message.Attachment attachment)
        {
            String filename = message.GetAttachmentFilenameFromSMIL(attachment);
            if(filename == null)
            {
                filename = $"Untitled attachment.{attachment.DataFileExtension}";
            }

            String ext = System.IO.Path.GetExtension(filename);

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = filename,
                DefaultExt = ext,
                Filter = $"{ext}|*{ext}"
            };
            if (dlg.ShowDialog() == true)
            {
                File.WriteAllBytes(dlg.FileName, attachment.data);
            }
        }

        private void SaveAttachment_Click(object sender, RoutedEventArgs e)
        {
            SaveAttachment((Message.Attachment)(((MenuItem)sender).Tag));
        }

        private void CopyImage_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetImage((BitmapImage)(((MenuItem)sender).Tag));
        }

        private void CopyText_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText((String)(((MenuItem)sender).Tag));
        }
    }
}
