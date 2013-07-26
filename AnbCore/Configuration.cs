using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AnbCore
{
    public class Configuration
    {
        public Configuration()
        {
            
        }

        public string Nick { get; set; }
        
        public int Port { get; set; }

        public string Ip { get; set; }

        public FileInfo KeyFile { get; set; }

        public List<Contact> Contacts { get; set; }

        public RSACryptoServiceProvider RsaCryptoServiceProvider { get; set; }

        public string HistoryPath { get; set; }
    }
}
