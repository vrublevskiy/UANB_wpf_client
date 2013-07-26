using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AnbCore
{
    public class AsyncClient
    {
        public event EventHandler<NotificationEventArgs> NotificationReceived;

        private ClientObject _cObject;
        private Configuration _configuration;
        private Thread _readThread;
        static UnicodeEncoding _byteConverter = new UnicodeEncoding();

        // ManualResetEvent instances signal completion.
        private readonly ManualResetEvent _connectDone =
            new ManualResetEvent(false);
        private readonly ManualResetEvent _sendDone =
            new ManualResetEvent(false);
        private readonly ManualResetEvent _receiveDone =
            new ManualResetEvent(false);

        protected virtual void OnNewNotification(NotificationEventArgs e)
        {
            EventHandler<NotificationEventArgs> temp = NotificationReceived;

            if (temp != null) temp(this, e);
        }

        #region Public functions

        public AsyncClient(Configuration configuration)
        {
            _configuration = configuration;
            _cObject = new ClientObject();
        }

        public void Register()
        {
            //  Creating TCP/IP socket
            _cObject.WorkSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //  Connect to the remote endpoint
            _cObject.WorkSocket.BeginConnect(_configuration.Ip, _configuration.Port, new AsyncCallback(ConnectCallback), _cObject);
            _connectDone.WaitOne();

            var pub = _configuration.RsaCryptoServiceProvider.ToXmlString(false);

            var startIndex = pub.IndexOf("<Modulus>") + 9;
            var endIndex = pub.IndexOf("</Modulus>");

            var len = endIndex - startIndex;

            var mod = pub.Substring(startIndex, len);

            Send(string.Format("sender:{0};receiver:;msgType:register;content:{1}", _configuration.Nick, mod));
            _sendDone.WaitOne();

            Receive();
            _receiveDone.WaitOne();
        }

        public void Start()
        {
            //  Creating TCP/IP socket
            _cObject.WorkSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //  Connect to the remote endpoint
            _cObject.WorkSocket.BeginConnect(_configuration.Ip, _configuration.Port, new AsyncCallback(ConnectCallback), _cObject);
            _connectDone.WaitOne();

            //Send("Request auth.");
            //_sendDone.WaitOne();
            _readThread = new Thread(() =>
                {
                    Receive();
                    _receiveDone.WaitOne();
                });
            _readThread.Start();
        }

        public void Send(string data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            _cObject.WorkSocket.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), _cObject);
        }

        public void Stop()
        {
            _readThread.Abort();
            _cObject.WorkSocket.Shutdown(SocketShutdown.Both);
            _cObject.WorkSocket.Close();
        }

        #endregion

        #region Private functions

        private void ConnectCallback(IAsyncResult ar)
        {
            //  Completing the connection
            var client = (ClientObject)ar.AsyncState;
            client.WorkSocket.EndConnect(ar);

            //  Signaling that the connection has been made
            _connectDone.Set();
        }

        private void SendCallback(IAsyncResult ar)
        {
            _sendDone.Set();
        }

        private void Receive()
        {
            _cObject.WorkSocket.BeginReceive(_cObject.Buffer, 0, _cObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), _cObject);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            if (!_cObject.WorkSocket.Connected) return;
            var bytesRead = _cObject.WorkSocket.EndReceive(ar);

            if (bytesRead > 0)
            {
                _cObject.Content.Append(Encoding.ASCII.GetString(_cObject.Buffer, 0, bytesRead));
                _cObject.WorkSocket.BeginReceive(_cObject.Buffer, 0, _cObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), _cObject);

                if (bytesRead < 256)
                {
                    _receiveDone.Set();

                    OnNewNotification(new NotificationEventArgs(_cObject.Content.ToString()));
                }
            }
            else
            {
                //  All data arrived
                _receiveDone.Set();
            }
        }

        #endregion

        #region Properties

        public Configuration Configuration { get { return _configuration; }}

        #endregion
    }
}
