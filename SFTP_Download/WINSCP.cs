using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;
using WinSCP;
using System.Text;
using System.IO;

namespace SFTP_Synchronize
{
    /// <summary>
    /// Performs the syncronization (uploading and downloading) of directories (local and ftp)
    /// </summary>
    public class WINSCP
    {

        /// <summary>
        /// Performs the syncronization (uploading and downloading) of directories (local and ftp) based on InputParameters set
        /// </summary>
        /// <param name="Parameters">Holds the properties needed for the synchronization</param>
        /// <returns>0 if successfull, 1 if error</returns>
        public int SynchronizeDirectories(InputParameters Parameters)
        {
            // Setup Event Log
            EventLogging.AppEventLog.Source = Parameters.AppLogSourceName;
            EventLogging.AppLoggingEventId = Parameters.AppLoggingEventId;
            EventLogging.AppLoggingCategory = Parameters.AppLoggingCategory;

            // Check if Downloading or Uploading
            bool IsDownloading = Parameters.UsedForDownloading.Equals("TRUE") ? true : false;

            // Check if to delete file afterwards
            bool DeleteFiles = Parameters.DeleteFiles.Equals("TRUE") ? true : false;

            string ActionVerb = IsDownloading ? "Downloading" : "Uploading";
            SynchronizationMode SyncMode = IsDownloading ? SynchronizationMode.Local : SynchronizationMode.Remote;

            // Setup session options
            SessionOptions sessionOptions = new SessionOptions
            {
                Protocol = Protocol.Sftp,
                HostName = Parameters.HostName,
                UserName = Parameters.UserName,
                SshHostKeyFingerprint = Parameters.SshHostKeyFingerprint,
                SshPrivateKeyPath = Parameters.SshPrivateKeyPath
            };
            StringBuilder messages = new StringBuilder();
            string message = "";

            try
            {
                using (Session session = new Session())
                {
                    string FTPFolder = Parameters.FTPDownloadFolder;
                    string LocalPath = Parameters.LocalDestinationPath;
                    bool FoundFilesToSync = false;

                    EventLogging.WriteEntry("Before Connecting for "+ ActionVerb + " - " + sessionOptions.HostName + " : " + sessionOptions.UserName, EventLogEntryType.Information);

                    // Connect
                    session.Open(sessionOptions);

                    message = "Connected for " + ActionVerb + " from "+ (IsDownloading ? "FTP Folder " + FTPFolder + " to Local Folder " + LocalPath : "Local Folder  " + LocalPath+ " to FTP Folder " + FTPFolder);
                    EventLogging.WriteEntry(message, EventLogEntryType.Information);

                    // Will continuously report progress of synchronization
                    session.FileTransferred += FileTransferred;

                    // Synchronize Directories
                    SynchronizationResult synchronizationResult = session.SynchronizeDirectories(SyncMode, LocalPath, FTPFolder, DeleteFiles);
                    
                    // Throw on any error
                    synchronizationResult.Check();

                    // Check if downloaded anything and delete remote file
                    foreach (TransferEventArgs Download in synchronizationResult.Downloads)
                    {
                        FoundFilesToSync = true;

                        // log successfull message
                        message = "Success in " + ActionVerb + " file '" + Download.FileName + "' to Local Folder " + Download.Destination;
                        EventLogging.WriteEntry(message, EventLogEntryType.SuccessAudit);

                        // remember messages for email sending
                        messages.AppendLine(message);

                        // delete remote file
                        if (DeleteFiles)
                        {
                            session.RemoveFiles(Download.FileName);
                            message = "Success in deleting remote file '" + Download.FileName + "'";
                            EventLogging.WriteEntry(message, EventLogEntryType.SuccessAudit);
                            messages.AppendLine(message);
                        }
                    }

                    // Check if uploaded anything and delete local file
                    foreach (TransferEventArgs Upload in synchronizationResult.Uploads)
                    {
                        FoundFilesToSync = true;

                        // log successfull message
                        message = "Success in " + ActionVerb + " file '" + Upload.FileName + "' to FTP folder " + Upload.Destination;
                        EventLogging.WriteEntry(message, EventLogEntryType.SuccessAudit);

                        // remember messages for email sending
                        messages.AppendLine(message);

                        // delete local file
                        if (DeleteFiles && File.Exists(Upload.FileName))
                        {
                            File.Delete(Upload.FileName);                         
                            message = "Success in deleting local file '" + Upload.FileName + "'";
                            EventLogging.WriteEntry(message, EventLogEntryType.SuccessAudit);
                            messages.AppendLine(message);
                        }
                    }

                    // Send Email
                    if (FoundFilesToSync)
                        SendEmail.Send(true, Parameters, messages);
                    else
                    {
                        // nothing different found
                        message = "No files found for " + ActionVerb;
                        EventLogging.WriteEntry(message, EventLogEntryType.SuccessAudit);
                    }
                }

                return 0;
            }
            catch (Exception e)
            {
                // log Error message
                message = "Error in " + ActionVerb + " : " + e.ToString();
                EventLogging.WriteEntry(message, EventLogEntryType.Error);

                messages.AppendLine(message);
                SendEmail.Send(false, Parameters, messages);
                
                return 1;
            }
        }

        /// <summary>
        /// WINSCP API function for logging
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileTransferred(object sender, TransferEventArgs e)
        {
            if (e.Error == null)
            {
                Console.WriteLine("Upload of {0} succeeded", e.FileName);
            }
            else
            {
                Console.WriteLine("Upload of {0} failed: {1}", e.FileName, e.Error);
            }

            if (e.Chmod != null)
            {
                if (e.Chmod.Error == null)
                {
                    Console.WriteLine(
                        "Permissions of {0} set to {1}", e.Chmod.FileName, e.Chmod.FilePermissions);
                }
                else
                {
                    Console.WriteLine(
                        "Setting permissions of {0} failed: {1}", e.Chmod.FileName, e.Chmod.Error);
                }
            }
            else
            {
                Console.WriteLine("Permissions of {0} kept with their defaults", e.Destination);
            }

            if (e.Touch != null)
            {
                if (e.Touch.Error == null)
                {
                    Console.WriteLine(
                        "Timestamp of {0} set to {1}", e.Touch.FileName, e.Touch.LastWriteTime);
                }
                else
                {
                    Console.WriteLine(
                        "Setting timestamp of {0} failed: {1}", e.Touch.FileName, e.Touch.Error);
                }
            }
            else
            {
                // This should never happen during "local to remote" synchronization
                Console.WriteLine(
                    "Timestamp of {0} kept with its default (current time)", e.Destination);
            }
        }
    }
}
