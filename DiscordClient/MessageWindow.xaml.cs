using DSharpPlus;
using DSharpPlus.Entities;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using System.Windows.Shapes;

namespace DBClient
{
    /// <summary>
    /// Interaction logic for MessageWindow.xaml
    /// </summary>
    public partial class MessageWindow : MetroWindow
    {
        private static SortedDictionary<ulong, MessageWindow> active = new SortedDictionary<ulong, MessageWindow>();
        GuildView Parent;
        DiscordClient Client;
        DiscordMember Member;
        DiscordMessage Message;
        DiscordMember Current;
        public MessageWindow(GuildView parent, DiscordClient client, DiscordMember member, DiscordMessage message)
        {
            InitializeComponent();
            this.ResizeMode = ResizeMode.NoResize;
            this.Title = $"Mensaje enviado en {message.Channel.Guild.Name} -> {message.Channel.Name}";
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.Current = parent.getMember(client.CurrentUser.Id);
            this.Parent = parent;
            this.Client = client;
            this.Member = member;
            this.Message = message;

            active.Add(message.Id, this);

            Update();

            btn_CopyId.Click += (sender, e) =>
            {

                System.Windows.Clipboard.SetText(Message.Id.ToString());

            };

            btn_Delete.Click += (sender, e) =>
            {

                try
                {
                    message.DeleteAsync().GetAwaiter().GetResult();
                }
                catch
                {

                }
                this.Close();
            };

            btn_Edit.Click += (sender, e) =>
            {
                if (message_Text.Text == message.Content)
                {
                    MessageBox.Show("No se pudo editar el mensaje por que es igual que el original.");
                    return;
                }

                try
                {
                    message.ModifyAsync(x => x.Content = message_Text.Text).GetAwaiter().GetResult();
                }catch(Exception err)
                {
                    MessageBox.Show("Error: " + err.Message);
                }
                this.Close();

            };


            btn_Save.Click += (sender, e) =>
            {
                MessageBox.Show("TO DO");
            };

            btn_MemberInfo.Click += (sender, e) =>
            {
                MessageBox.Show("TO DO");

            };
            btn_GuildInfo.Click += (sender, e) =>
            {
                MessageBox.Show("TO DO");

            };
            btn_ChannelInfo.Click += (sender, e) =>
            {
                MessageBox.Show("TO DO");

            };

            this.Closing += MessageWindow_Closing;
        }

        public static void Show(GuildView parent, DiscordClient client, DiscordMember member, DiscordMessage message)
        {
            if (active.ContainsKey(message.Id))
            {
                active[message.Id].Activate();
                active[message.Id].Topmost = true;
                active[message.Id].Topmost = false;
                active[message.Id].Focus();
                return;
            }
            else
            {
                if (active.Count > 4)
                {
                    active.Last().Value.Close();
                }

                MessageWindow w = new MessageWindow(parent, client, member, message);
                w.Show();
            }
        }

        private void MessageWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            active.Remove(this.Message.Id);
        }
    
        private void Update()
        {
            member_Name.Content = Member.Nickname == null ? Member.Username : Member.Nickname;


            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(Message.Author.AvatarUrl, UriKind.Absolute);
            bitmap.EndInit();


            member_profilePicture.Source = bitmap;
            member_profilePicture.Clip = new EllipseGeometry(new Point(50,50), 50, 50);

            message_Text.Text = Message.Content;


            if (Message.Author.Id != Current.Id && !Current.Permissions.HasPermission(Permissions.ManageMessages))
            {
                btn_Delete.IsEnabled = false;
            }

            if (Message.Author.Id != Current.Id)
            {
                btn_Edit.IsEnabled = false;
            }
            else
            {
                message_Text.IsReadOnly = false;
            }


        }

    }
}
