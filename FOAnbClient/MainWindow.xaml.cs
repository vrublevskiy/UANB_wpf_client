using AnbCore;
using FOAnbClient.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
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

namespace FOAnbClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly AsyncClient _client;

        public MainWindow(Configuration config)
        {
            InitializeComponent();

            _client = new AsyncClient(config);
            _client.Start();
            _client.NotificationReceived += _client_NotificationReceived;
            SignIn();
        }

        void _client_NotificationReceived(object sender, NotificationEventArgs e)
        {
            
        }

        private void SignIn()
        {
            var pub = _client.Configuration.RsaCryptoServiceProvider.ToXmlString(false);

            var startIndex = pub.IndexOf("<Modulus>") + 9;
            var endIndex = pub.IndexOf("</Modulus>");

            var len = endIndex - startIndex;

            var mod = pub.Substring(startIndex, len);

            _client.Send(string.Format("sender:{0};receiver:;msgType:signin;content:{1}", _client.Configuration.Nick, mod));
        }

        protected override void OnClosed(EventArgs e)
        {
            _client.Stop();

            base.OnClosed(e);
        }
    }
}
