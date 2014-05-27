/** 
 * This file is part of the OrderImportErrorWatcher project.
 * Copyright (c) 2014 Dai Nguyen
 * Author: Dai Nguyen
**/

namespace OrderImportErrorWatcher.Models
{
    public class Config : SmtpConfig
    {
        public int WaitInSeconds { get; set; }
        public string ActiveFolder { get; set; }
        public string ErrorFolder { get; set; }
        public string SummaryFolder { get; set; }        
    }

    public class SmtpConfig
    {
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUser { get; set; }
        public string SmtpPass { get; set; }
        public bool EnableSSL { get; set; }
        public string EmailFrom { get; set; }
        public string[] EmailTos { get; set; }
    }
}
