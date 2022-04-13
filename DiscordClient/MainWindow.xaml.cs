using DSharpPlus;
using MahApps.Metro.Controls;
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

namespace DBClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public static readonly String PathV = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DiscordClient");
        public MainWindow()
        {
            InitializeComponent();

            if (!System.IO.Directory.Exists(PathV))
            {
                System.IO.Directory.CreateDirectory(PathV);
            }
            this.Token.Text = GetLastTokenSaved();
            this.Connect.IsEnabled = false;
            if (this.Token.Text != "" && this.Token.Text != null)
            {
                this.Connect.IsEnabled = true;
            }
            this.Title = "Token";
            this.ResizeMode = ResizeMode.NoResize;
            this.Token.TextChanged += (text, e) =>
            {
                if (this.Token.Text.Trim() == "")
                {
                    this.Connect.IsEnabled = false;
                    return;
                }
                else
                {
                    if (this.Token.Text.Contains(" ") || this.Token.Text.Contains("\n"))
                    {
                        MessageBox.Show("Espacios no permitidos");
                        this.Token.Text = this.Token.Text.Trim();
                    }

                    this.Connect.IsEnabled = true;
                }
            };

            this.Connect.Click += (a, e) =>
            {
                this.Token.IsEnabled = false;
                this.Connect.IsEnabled = false;


                DSharpPlus.DiscordClient client = new DSharpPlus.DiscordClient(new DiscordConfiguration
                {
                    AutoReconnect = true,
                    Token = this.Token.Text
                });
                try
                {

                    client.ConnectAsync().GetAwaiter().GetResult();

                }
                catch (Exception error)
                {
                    MessageBox.Show("Token invalida");

                    client.Dispose();

                    this.Token.IsEnabled = true;
                    this.Connect.IsEnabled = true;
                    return;
                }


                SetLastToken(this.Token.Text);
                ClientWindow w = new ClientWindow(client);

                w.Show();

                this.Close();

            };
        }

        private string GetLastTokenSaved()
        {
            if (System.IO.File.Exists(System.IO.Path.Combine(PathV, "TokenHistory.txt")))
            {
                return File.ReadLines(System.IO.Path.Combine(PathV, "TokenHistory.txt")).LastOrDefault();
            }

            return "";
        }

        private void SetLastToken(String tkn)
        {
            if (!File.Exists(System.IO.Path.Combine(PathV, "TokenHistory.txt")))
            {
                File.WriteAllText(System.IO.Path.Combine(PathV, "TokenHistory.txt"), tkn);
            }
            else
            {
                var lines = File.ReadAllLines(System.IO.Path.Combine(PathV, "TokenHistory.txt")).ToList();

                if (lines.Count == 0)
                {
                    File.WriteAllText(System.IO.Path.Combine(PathV, "TokenHistory.txt"), tkn);
                    return;
                }

                if (lines.Last() == tkn)
                {
                    return;
                }
                lines.Add(tkn);
                File.WriteAllLines(System.IO.Path.Combine(PathV, "TokenHistory.txt"), lines.ToArray());


            }
        }
    }
}
