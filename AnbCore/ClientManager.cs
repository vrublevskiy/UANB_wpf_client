using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnbCore
{
    public static class ClientManager
    {
        private static AsyncClient _client;

        static ClientManager()
        {

        }

        public static void Configure(Configuration config)
        {
            _client = new AsyncClient(config);
        }

        public static void Start()
        {
            Client.Start();
        }

        #region Properties

        public static AsyncClient Client { get { return _client; } }

        #endregion
    }
}
