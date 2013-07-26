using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
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
using AnbCore;
using Microsoft.Win32;

namespace FOAnbClient
{
    /// <summary>
    /// Interaction logic for Start.xaml
    /// </summary>
    public partial class Start : Window
    {
        private AsyncClient _client;

        private bool _configReady = false;
        private Configuration _config = new Configuration();

        public Start()
        {
            InitializeComponent();
            Loaded += Start_Loaded;
        }

        void Start_Loaded(object sender, RoutedEventArgs e)
        {
            ValidateConfig();
        }

        private void ValidateConfig()
        {
            _configReady = InitKey() && InitHistory() && InitServer();
        }

        private bool InitKey()
        {
            var keyPath = Application.Current.Properties["KeyPath"];

            if (keyPath != null)
                TbKey.Text = keyPath.ToString();

            try
            {
                string keyStr;

                using (var reader = new StreamReader(new FileStream(keyPath.ToString(), FileMode.Open)))
                {
                    keyStr = reader.ReadToEnd();
                }

                //  Setup rsa configuration
                _config.RsaCryptoServiceProvider = new RSACryptoServiceProvider();
                _config.RsaCryptoServiceProvider.FromXmlString(keyStr);
            }
            catch
            {
                TblockStatus.Text = "Invalid key file";

                return false;
            }

            TblockStatus.Text = "Ready to connect";
            return true;
        }

        private bool InitHistory()
        {
            var hisPath = Application.Current.Properties["HistoryPath"];

            if (hisPath != null)
            {
                _config.HistoryPath = hisPath.ToString();
                TbHistory.Text = _config.HistoryPath;

                TblockStatus.Text = "Ready to connect";
                return true;
            }

            TblockStatus.Text = "Invalid history path.";
            return false;
        }

        private bool InitServer()
        {
            var sb = new StringBuilder();
            var buf = new byte[1024];
            var request = (HttpWebRequest)WebRequest.Create("http://anb.vrublevskiy.org/anb.txt");

            // execute the request
            HttpWebResponse response = (HttpWebResponse)
                request.GetResponse();
            // we will read data via the response stream
            Stream resStream = response.GetResponseStream();
            string tempString = null;
            int count = 0;

            do
            {
                // fill the buffer with data
                count = resStream.Read(buf, 0, buf.Length);

                // make sure we read some data
                if (count != 0)
                {
                    // translate from bytes to ASCII text
                    tempString = Encoding.ASCII.GetString(buf, 0, count);

                    // continue building the string
                    sb.Append(tempString);
                }
            }
            while (count > 0);

            var lines = sb.ToString().Split(new[] { ';' });

            try
            {
                _config.Ip = lines[0].Split(new[] { ':' })[1];
                _config.Port = int.Parse(lines[1].Split(new[] { ':' })[1]);
            }
            catch
            {
                return false;
            }

            return true;
        }

        private void BtnKey_OnClick(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.DefaultExt = "*.key";
            dlg.Filter = "Key documents (.key)|*.key";

            var dlgResult = dlg.ShowDialog();

            if (dlgResult.HasValue && dlgResult.Value)
            {
                Application.Current.Properties["KeyPath"] = dlg.FileName;

                ValidateConfig();
            }
        }

        private void BtnHistory_OnClick(object sender, RoutedEventArgs e)
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog();

            var result = dlg.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                Application.Current.Properties["HistoryPath"] = dlg.SelectedPath;
            }

            ValidateConfig();
        }

        private void BtnRegister_OnClick(object sender, RoutedEventArgs e)
        {
            if (TbNickname.Text == string.Empty)
            {
                MessageBox.Show("Nickname required.");
                return;
            }

            _config.Nick = TbNickname.Text;
            _client = new AsyncClient(_config);
            _client.NotificationReceived += client_NotificationReceived;
            _client.Register();
        }

        void client_NotificationReceived(object sender, NotificationEventArgs e)
        {
            _client.Stop();

            if (e.Message == "0")
                MessageBox.Show("Nickname is already in use :(");
            else
            {
                MessageBox.Show("You've registered a new account :)");
            }
        }

        private void BtnGenKey_OnClick(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.Filter = "Key documents (.key)|*.key";

            var result = dlg.ShowDialog();
            if (result.HasValue && result.Value)
            {
                var key = string.Empty;

                using (var rsa = new RSACryptoServiceProvider())
                {
                    key = rsa.ToXmlString(true);
                }

                using (var writer = new StreamWriter(new FileStream(dlg.FileName, FileMode.Truncate)))
                {
                    writer.Write(key);
                }

                TbKey.Text = dlg.FileName;
                Application.Current.Properties["KeyPath"] = dlg.FileName;

                ValidateConfig();
            }
        }

        private void BtnConnect_OnClick(object sender, RoutedEventArgs e)
        {
            if (TbNickname.Text == string.Empty)
            {
                MessageBox.Show("Nickname required.");
                return;
            }

            _config.Nick = TbNickname.Text;
            new MainWindow(_config).Show();
            
            this.Close();
        }
    }
}
