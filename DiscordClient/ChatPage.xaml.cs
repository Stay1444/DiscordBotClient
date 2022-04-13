using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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

namespace DBClient
{
    /// <summary>
    /// Interaction logic for ChatPage.xaml
    /// </summary>
    public partial class ChatPage : Page
    {

        private struct MessageData
        {
            public ulong Id;
            public DateTime D;
            public bool E;
        }


        private DiscordClient client;
        private ulong Id;
        private DiscordChannel Channel;
        private GuildView parent;
        private System.Timers.Timer UpdateTimer;
        public ChatPage(DiscordClient client, DiscordChannel channel, GuildView parent, ulong channelId)
        {
            this.parent = parent;
            this.Id = channelId;
            this.Channel = channel;
            this.client = client;
            InitializeComponent();
            this.chatbox.KeyDown += Chatbox_KeyDown;
            client.MessageCreated += Client_MessageCreated;
            client.MessageUpdated += Client_MessageUpdated;
            Task.Run(() => LoadPastMessages());
            UpdateTimer = new System.Timers.Timer(60000);
            UpdateTimer.Enabled = true;
            UpdateTimer.AutoReset = true;
            UpdateTimer.Elapsed += UpdateTimer_Elapsed;
            SVP.ScrollChanged += SVP_ScrollChanged;
            client.MessageDeleted += Client_MessageDeleted;

        }

        private void SVP_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalOffset == 0 && SVP.ScrollableHeight > 1)
            {
                Application.Current.Dispatcher.Invoke((Action)delegate {

                    SVP.ScrollChanged -= SVP_ScrollChanged;

                    double lastHeight = SVP.ScrollableHeight;

                    ulong from = (ulong)((Grid)Messages.Children[0]).Tag;

                    var messages = Channel.GetMessagesBeforeAsync(from, 5).Result;

                    foreach (var item in messages)
                    {
                        CreateMessage(parent.getMember(item.Author.Id), item, 0);
                    }

                    double diff = SVP.ScrollableHeight - lastHeight;

                    SVP.ScrollToVerticalOffset(521);

                    SVP.ScrollChanged += SVP_ScrollChanged;


                });
            }
        }

        private void UpdateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate {

                foreach (Grid m in Messages.Children)
                {
                    
                    Label data = m.Children[2] as Label;
                    MessageData mdata = (MessageData)data.Tag;
                    data.Content = $"{(mdata.E ? "[EDITADO]" : "")} {toHumanData(mdata.D)}";
                    
                }

            });
        }

        private async Task Client_MessageUpdated(DiscordClient sender, DSharpPlus.EventArgs.MessageUpdateEventArgs data)
        {
            if (data.Channel.Id != Id) { return; }


            Application.Current.Dispatcher.Invoke((Action)delegate {

                try
                {

                    DiscordMember member = parent.getMember(data.Author.Id);

                    Grid messageGrid = null;
                    foreach (var item in Messages.Children)
                    {
                        Grid t = item as Grid;
                        ulong adi = (ulong)t.Tag;
                        if (adi == data.Message.Id)
                        {
                            messageGrid = t;

                            break;
                        }
                        else
                        {
                        }
                    }
                    if (messageGrid == null) { return; }

                    Label title = messageGrid.Children[2] as Label;
                    title.Tag = new MessageData { Id = data.Message.Id, D = data.Message.EditedTimestamp.Value.LocalDateTime, E = true };
                    title.Content = $"[EDITADO] {toHumanData(data.Message.EditedTimestamp.Value.LocalDateTime)}";
                    TextBlock messagecontent = messageGrid.Children[1] as TextBlock;
                    RenderText(data.Message.Content, messagecontent);

                }
                catch { }

            });
        }


        private void RenderText(String Text, TextBlock block)
        {
            try
            {

                block.Text = "";
                foreach (String word in Text.Split(" "))
                {
                    Run data = new Run(word);
                    block.Inlines.Add(data);
                    ProcessText(data, word);
                    data.Text += " ";

                }


            }
            catch(Exception error)
            {
                block.Text = $"ERROR OCURRED WHILE RENDERING TEXT: {error} ORIGINAL TEXT: {Text}";
            }
        }

        private static Regex r_UserMention = new Regex("<@!?[0-9]*>", RegexOptions.Compiled);
        private Run ProcessText(Run data, String text)
        {

            if (r_UserMention.IsMatch(text))
            {
                string ulongText = text.Replace("<", "").Replace(">", "").Replace("!", "").Replace("@", "");

                if (ulong.TryParse(ulongText, out ulong id))
                {
                    DiscordMember member = parent.getMember(id);
                    data.Foreground = new SolidColorBrush( Color.FromRgb(member.Color.R, member.Color.G, member.Color.B));
                    data.Text = $"@{(member.Nickname == null ? member.Username : member.Nickname)}";
                
                
                    if (id == client.CurrentUser.Id)
                    {
                        ((TextBlock)data.Parent).Background = new SolidColorBrush(Color.FromArgb(20, Colors.Gold.R, Colors.Gold.G, Colors.Gold.B));
                    }

                }

            }

            if (Uri.TryCreate(text, UriKind.Absolute, out Uri uri))
            {
                data.Foreground = new SolidColorBrush(Colors.CornflowerBlue);
                data.TextDecorations.Add(TextDecorations.Underline);
            }

            return data;

        }

        private void URL_MouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Arrow;

        }

        private void URL_MouseEnter(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private async Task Client_MessageCreated(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs data)
        {
            if (data.Channel.Id != Id) { return; }


            Application.Current.Dispatcher.Invoke((Action)delegate {

                try
                {
                    CreateMessage(parent.getMember(data.Author.Id), data.Message);
                }
                catch { }

            });
        }

        public void Dispose()
        {
            client.MessageCreated -= Client_MessageCreated;
            client.MessageUpdated -= Client_MessageUpdated;
            client.MessageDeleted -= Client_MessageDeleted;
            UpdateTimer.Enabled = false;
            UpdateTimer.AutoReset = false;
            UpdateTimer.Stop();
            UpdateTimer.Dispose();
        }

        private async Task Client_MessageDeleted(DiscordClient sender, DSharpPlus.EventArgs.MessageDeleteEventArgs data)
        {
            if (data.Channel.Id != Id) { return; }


            Application.Current.Dispatcher.Invoke((Action)delegate {

                try
                {


                    Grid messageGrid = null;
                    foreach (var item in Messages.Children)
                    {
                        Grid t = item as Grid;
                        ulong adi = (ulong)t.Tag;
                        if (adi == data.Message.Id)
                        {
                            messageGrid = t;

                            break;
                        }
                        else
                        {
                        }
                    }
                    if (messageGrid == null) { return; }

                    Messages.Children.Remove(messageGrid);

                }
                catch { }

            });
        }

        private void Chatbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            if (this.chatbox.Text.Trim() != "")
            {
                Channel.SendMessageAsync(this.chatbox.Text);
                this.chatbox.Text = "";
            }
        }

        private async Task LoadPastMessages()
        {

            new Thread(() =>
            {
                var messages = Channel.GetMessagesAsync(25).Result;
                var messagesl = messages.Reverse().ToList();
                for (int i = 0; i < messagesl.Count; i++)
                {

                    try
                    {


                        CreateMessage(parent.getMember(messagesl[i].Author.Id), messagesl[i]);
                    }
                    catch { }

                }
            }).Start();

        }

        private void CreateMessage(DiscordMember member, DiscordMessage message, int? index = null)
        {
            try
            {
                if (member == null)
                    return;
                Application.Current.Dispatcher.Invoke((Action)delegate {


                    Grid newMessage = new Grid();

                    newMessage.ColumnDefinitions.Add(new ColumnDefinition
                    {
                        Width = new GridLength(60)
                    });
                    newMessage.ColumnDefinitions.Add(new ColumnDefinition
                    {

                    });

                    newMessage.RowDefinitions.Add(new RowDefinition
                    {
                        Height = new GridLength(60)
                    });
                    newMessage.RowDefinitions.Add(new RowDefinition
                    {

                    });

                    newMessage.Margin = new Thickness(0, 0, 0, 25);

                    {
                        ContextMenu main = new ContextMenu();
                        newMessage.ContextMenu = main;

                        MenuItem ctx_copy = new MenuItem();
                        main.Items.Add(ctx_copy);

                        ctx_copy.Header = "Copiar";

                        MenuItem ctx_mention = new MenuItem();
                        main.Items.Add(ctx_mention);

                        ctx_mention.Header = "Mencionar";

                        ctx_mention.Click += (a, b) =>
                        {
                            this.chatbox.Text += message.Author.Mention.ToString();
                        };

                        MenuItem ctx_CopyId = new MenuItem();
                        main.Items.Add(ctx_CopyId);
                        ctx_CopyId.Header = "Copiar Id";

                        if (message.Author.Id == client.CurrentUser.Id || this.parent.getCurrentUser().Permissions.HasPermission(Permissions.ManageMessages))
                        {
                            MenuItem ctx_delete = new MenuItem();
                            main.Items.Add(ctx_delete);
                            ctx_delete.Header = "Eliminar";
                            ctx_delete.Background = new SolidColorBrush(Colors.IndianRed);

                            ctx_delete.Click += (a, b) =>
                            {
                                try
                                {
                                    message.DeleteAsync().GetAwaiter().GetResult();
                                }
                                catch(Exception error)
                                {
                                    MessageBox.Show(error.Message);
                                }
                            };
                        }

                        Separator separator = new Separator();
                        main.Items.Add(separator);

                        MenuItem ctx_details = new MenuItem();
                        ctx_details.Header = "Detalles";
                        main.Items.Add(ctx_details);

                        ctx_details.Click += (a, b) =>
                        {
                            MessageWindow.Show(this.parent, client, member, message);
                        };

                        ctx_copy.Click += (a, b) =>
                        {
                            Clipboard.SetText(message.Content);
                        };

                        ctx_CopyId.Click += (a, b) =>
                        {
                            Clipboard.SetText(message.Id.ToString());
                        };
                    }

                    Image image = new Image();
                    image.SetValue(Grid.ColumnProperty, 0);
                    image.Clip = new EllipseGeometry(new Point(25, 25), 25, 25);
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(message.Author.AvatarUrl, UriKind.Absolute);
                    bitmap.EndInit();
                    image.Source = bitmap;

                    newMessage.Children.Add(image);

                    image.Width = 50;
                    image.Height = 50;
                    image.Margin = new Thickness(5, 5, 5, 5);


                    TextBlock text = new TextBlock();
                    text.Foreground = new SolidColorBrush(Colors.White);
                    text.SetValue(Grid.ColumnProperty, 1);
                    RenderText(message.Content, text);
                    text.Margin = new Thickness(10, 32, 0, 0);
                    text.TextWrapping = TextWrapping.Wrap;
                    newMessage.Children.Add(text);


                    Label data = new Label();
                    Label nam = new Label();

                    string timesp = "";

                    if (message.IsEdited)
                    {
                        timesp = "[EDITADO] " + toHumanData(message.EditedTimestamp.Value.LocalDateTime);
                    }
                    else
                    {
                        timesp = toHumanData(message.Timestamp.LocalDateTime);
                    }

                    nam.Content = $"{(member.Nickname == null ? member.Username : member.Nickname)}";
                    data.Content = timesp;
                    newMessage.Children.Add(data);

                    nam.SetValue(Grid.ColumnProperty, 1);

                    newMessage.Children.Add(nam);

                    newMessage.Margin = new Thickness(0, 10, 0, 0);
                    nam.FontSize = 16;
                    nam.Tag = member.Id;
                    data.FontSize = 16;
                    data.SetValue(Grid.ColumnProperty, 1);
                    data.HorizontalContentAlignment = HorizontalAlignment.Right;
                    nam.Foreground = new SolidColorBrush(Color.FromRgb(member.Color.R, member.Color.G, member.Color.B));
                    nam.Margin = new Thickness(5, 0, 0, 0);
                    text.FontSize = 16;
                    text.SetValue(Grid.RowSpanProperty, 2);
                    newMessage.Tag = message.Id;

                    if (message.Attachments.Count >= 1)
                    {
                        if (IsImageUrl(message.Attachments[0].Url))
                        {
                            Image img = new Image();

                            BitmapImage d = new BitmapImage();
                            d.BeginInit();
                            d.UriSource = new Uri(message.Attachments[0].Url);
                            d.EndInit();
                            img.Width = Math.Max((double)message.Attachments[0].Width, this.parent.Width / 2);
                            img.Height = Math.Max((double)message.Attachments[0].Height, this.parent.Height / 2);
                            img.Source = d;
                            img.SetValue(Grid.RowProperty, 2);
                            img.SetValue(Grid.ColumnProperty, 1);

                            newMessage.Children.Add(img);

                        }
                    }
                    data.Tag = new MessageData { Id = message.Id, D = (message.IsEdited ? message.EditedTimestamp.Value.LocalDateTime : message.CreationTimestamp.LocalDateTime), E = message.IsEdited };
                    if (index == null)
                        Messages.Children.Add(newMessage);
                    else
                        Messages.Children.Insert((int)index, newMessage);
                    ScrollViewer parent = Messages.Parent as ScrollViewer;

                    double scrollOffset = parent.ScrollableHeight - parent.VerticalOffset;
                    if (scrollOffset < 100)
                    {
                        parent.ScrollToEnd();
                    }
                });

            }
            catch { }

        }

        private void MSG_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Grid g = sender as Grid;
            DiscordMessage message = Channel.GetMessageAsync((ulong)g.Tag).Result; 
            MessageWindow.Show(parent, client, parent.getMember(message.Author.Id), message);
            e.Handled = true;
        }

        bool IsImageUrl(string URL)
        {
            var req = (HttpWebRequest)HttpWebRequest.Create(URL);
            req.Method = "HEAD";
            using (var resp = req.GetResponse())
            {
                return resp.ContentType.ToLower(CultureInfo.InvariantCulture)
                           .StartsWith("image/");
            }
        }

        private string toHumanData(DateTime time)
        {
            string result = time.ToShortDateString();
            TimeSpan span = DateTime.Now - time;
            if (span.TotalMinutes < 1)
            {
                return "Ahora";
            }

            if (span.TotalDays <= 1)
            {
                string r = "";
                if (span.TotalHours > 1)
                {
                    return "Hace " + Math.Round(span.TotalHours) + " horas";
                }

                if (span.TotalMinutes > 1)
                {
                    return "Hace " + Math.Round(span.TotalMinutes) + " minutos";
                }

            }



            return result;
        }
    }
}
