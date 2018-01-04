using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SFTP_Synchronize
{
    /// <summary>
    /// Holds the properties needed for this application to run (sftp details, email accounts etc.)
    /// The Application depends on a parameters text file to run. The text file holds these parameters needed for the corresponding process to execute. 
    /// The name of the text file is indifferent to the process.
    /// </summary>
    public class InputParameters
    {
        public string UsedForDownloading = "";
        public string HostName = "";
        public string UserName = "";
        public string SshPrivateKeyPath = "";
        public string SshHostKeyFingerprint = "";
        public string FTPDownloadFolder = "";
        public string LocalDestinationPath = "";
        public string DeleteFiles = "";
        public string AppLogSourceName = "";
        public int AppLoggingEventId = 0;
        public short AppLoggingCategory = 0;
        public string EmailHostServer = "";
        public int EmailHostPort = 0;
        public string EmailFrom = "";
        public string EmailFromPassword = "";
        public string EmailSubject = "";
        public string EmailRecipient = "";

        /// <summary>
        /// Read & Store needed parameters from text file
        /// </summary>
        /// <param name="PathTxtFile">Path of txt file</param>
        public InputParameters(string PathTxtFile)
        {
            Dictionary<string, string> Properties = null;

            // Process the list of files found in the directory
            string[] fileEntries = Directory.GetFiles(PathTxtFile);
            foreach (string fileName in fileEntries)
            {
                // Read needed parameters from text file
                if (fileName.EndsWith(".txt"))
                {
                    Properties = this.GetProperties(fileName);
                    break;
                }
            }

            // Store needed parameters            
            UsedForDownloading = Properties["UsedForDownloading"];
            DeleteFiles = Properties["DeleteFiles"];
            HostName = Properties["HostName"];
            UserName = Properties["UserName"];
            SshPrivateKeyPath = Properties["SshPrivateKeyPath"];
            SshHostKeyFingerprint = Properties["SshHostKeyFingerprint"];
            FTPDownloadFolder = Properties["FTPDownloadFolder"];
            LocalDestinationPath = Properties["LocalDestinationPath"];
            AppLogSourceName = Properties["AppLoggingName"];
            AppLoggingEventId = Int32.Parse(Properties["AppLoggingEventId"]);
            AppLoggingCategory = (short)Int16.Parse(Properties["AppLoggingCategory"]);
            EmailHostServer = Properties["EmailHostServer"];
            EmailHostPort = Int32.Parse(Properties["EmailHostPort"]);
            EmailFrom = Properties["EmailFrom"];
            EmailFromPassword = Properties["EmailFromPassword"];
            EmailSubject = Properties["EmailSubject"];
            EmailRecipient = Properties["EmailRecipient"];                
        }

        /// <summary>
        /// Reads properties from file and stores them into Dictionary
        /// </summary>
        /// <param name="path">Path of txt file</param>
        /// <returns>Dictionary with properties</returns>
        private Dictionary<string, string> GetProperties(string path)
        {
            string fileData = "";
            using (StreamReader sr = new StreamReader(path))
            {
                fileData = sr.ReadToEnd().Replace("\r", "");
            }
            Dictionary<string, string> Properties = new Dictionary<string, string>();
            string[] kvp;
            string[] records = fileData.Split("\n".ToCharArray());
            foreach (string record in records)
            {
                if (record.IndexOf("=") > 0)
                {
                    kvp = record.Split("=".ToCharArray());
                    Properties.Add(kvp[0].Trim(), kvp[1].Trim());
                }
            }
            return Properties;
        }
    }
}
