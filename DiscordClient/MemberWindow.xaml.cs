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
using System.Windows.Shapes;

namespace DBClient
{
    /// <summary>
    /// Interaction logic for MemberWindow.xaml
    /// </summary>
    public partial class MemberWindow : Window
    {
        public MemberWindow(ulong member, DiscordClient client, DiscordGuild guild)
        {
            InitializeComponent();
            this.SizeChanged += MemberWindow_SizeChanged;
        }

        private void MemberWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            test.Content = $"{e.NewSize.Width} {e.NewSize.Height}";
        }
    }
}
