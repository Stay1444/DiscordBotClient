using MahApps.Metro.Controls;
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
using System.Windows.Shapes;

namespace DBClient
{
    /// <summary>
    /// Interaction logic for ClientWindow.xaml
    /// </summary>
    public partial class ClientWindow : MetroWindow
    {
        DSharpPlus.DiscordClient client;
        ulong SelectedGuild = 0;
        public ClientWindow(DSharpPlus.DiscordClient T)
        {
            InitializeComponent();

            client = T;

            client.GuildDownloadCompleted += Client_GuildDownloadCompleted;
            this.MinHeight = 400;
            this.MinWidth = 600;
        }

        private Task Client_GuildDownloadCompleted(DSharpPlus.DiscordClient sender, DSharpPlus.EventArgs.GuildDownloadCompletedEventArgs e)
        {
            foreach (var item in client.Guilds)
            {
                Application.Current.Dispatcher.Invoke((Action)delegate {
                    Image butt = new Image();

                    butt.Clip = new EllipseGeometry(new Point(30,30), 30, 30);
                    butt.Width = 60;
                    butt.Height = 60;
                    butt.ToolTip = item.Value.Name;
                    butt.Margin = new Thickness(0, 0, 0, 10);
                    butt.Tag = item.Key;
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri((item.Value.IconUrl == null ? "https://upload.wikimedia.org/wikipedia/commons/thumb/a/a8/Circle_Davys-Grey_Solid.svg/1200px-Circle_Davys-Grey_Solid.svg.png" : item.Value.IconUrl), UriKind.Absolute);
                    bitmap.EndInit();
                    butt.Source = bitmap;
                    butt.MouseDown += Butt_Click;
                    StackPanel_Servers.Children.Add(butt);
                });
            }

            return Task.CompletedTask;
        }

        private void Butt_Click(object sender, RoutedEventArgs e)
        {
            Image butt = sender as Image;

            if (SelectedGuild == (ulong)butt.Tag)
            {
                return;
            }
            else
            {
                SelectedGuild = (ulong)butt.Tag;
                ContentView.NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden;

                if (ContentView.Content is not null)
                {
                    ((GuildView)ContentView.Content).Dispose();
                }

                ContentView.Content = new GuildView(client, SelectedGuild);
            }
        }
    }
}
