using System;
using System.Collections.Generic;
using System.Text;

namespace EVRSplitter
{
    public class ConfigModel
    {
        public string ConnectionString { get; set; }
        public string DestinationPath { get; set; }
        public string SMTPServer { get; set; }
        public string SendEmailFrom { get; set; }
        public string SendEmailTo { get; set; }
    }
}
