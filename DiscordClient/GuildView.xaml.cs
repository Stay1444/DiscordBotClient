using DSharpPlus;
using DSharpPlus.Entities;
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
using System.Linq;

namespace DBClient
{
    /// <summary>
    /// Interaction logic for GuildView.xaml
    /// </summary>
    public partial class GuildView : Page
    {

        private DiscordGuild guild;
        private DSharpPlus.DiscordClient client;
        private Dictionary<ulong, DiscordMember> MembersCache = new Dictionary<ulong, DiscordMember>();

        public DiscordMember getMember(ulong id)
        {
            try
            {

                if (MembersCache.ContainsKey(id))
                    return MembersCache[id];



                DiscordMember member = guild.GetMemberAsync(id).Result;
                MembersCache.Add(member.Id, member);
            }
            catch
            {
                MembersCache.Add(id, null);
                return null;
            }

            return guild.GetMemberAsync(id).Result;
        }

        public DiscordMember getCurrentUser()
        {
            return getMember(client.CurrentUser.Id);
        }

        public GuildView(DiscordClient client, ulong GuildId)
        {
            InitializeComponent();
            this.client = client;
            guild = client.GetGuildAsync(GuildId).GetAwaiter().GetResult();
            chatFrame.NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden;

            foreach (var item in guild.Channels.Where(x => x.Value.Type == ChannelType.Category).OrderBy(x => x.Value.Position))
            {
                TreeViewItem cat = new TreeViewItem();
                cat.Header =  item.Value.Name;
                cat.Focusable = false;
                StackPanel_Channels.Items.Add(cat);
                cat.Tag = item.Key;
                cat.Background = new SolidColorBrush(Colors.Transparent);

            }
            GTitle.Content = guild.Name;
            foreach (var item in guild.Channels.Where(X => X.Value.Type == ChannelType.Text).OrderBy(x => x.Value.Position))
            {
                TreeViewItem chan = new TreeViewItem();

                chan.Header = "#" + item.Value.Name;

                chan.Tag = item.Key;

                chan.Selected += Chan_Selected;
                chan.Background = new SolidColorBrush(Colors.Transparent);

                if (item.Value.ParentId != null)
                {

                    TreeViewItem category = getCategory((ulong)item.Value.ParentId);
                    category.Items.Add(chan);
                }
            }
        }

        public void Dispose()
        {
            if (chatFrame.Content is not null)
                ((ChatPage)chatFrame.Content).Dispose();
        }

        private void Chan_Selected(object sender, RoutedEventArgs e)
        {
            ulong id = (ulong)((TreeViewItem)sender).Tag;

            if (chatFrame.Content != null)
            {
                ChatPage cu = chatFrame.Content as ChatPage;

                cu.Dispose();
            }

            chatFrame.Content = new ChatPage(client, client.GetChannelAsync(id).GetAwaiter().GetResult(), this, id);
        }

        private TreeViewItem getCategory(ulong id)
        {
            foreach (var item in StackPanel_Channels.Items)
            {
                TreeViewItem view = item as TreeViewItem;
                if ((ulong)view.Tag == id)
                {
                    return view;
                }
            }
            return null;
        }
    }
}
