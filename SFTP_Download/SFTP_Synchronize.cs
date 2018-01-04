using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace SFTP_Synchronize
{
    /// <summary>
    /// SFTP Synchronize is a console Application to Synchronize Local and Remote folders with SFTP, using WinSCP ( both uploading and downloading files ).
    /// The Application depends on a parameters text file to run. The text file holds parameters needed for the corresponding process to execute, such as sftp details, email accounts etc. 
    /// The name of the text file is indifferent to the process.
    /// The output of each process is logged. Windows Event Viewer or other Event Log viewers (i.e. NetCrunch WMI Tool) can be used for viewing the log.
    /// An email is sent if there are new files that have been Synchronized, or if there is an error in the process. No email is sent if there are no new files found.
    /// Library WinSCPnet.dll is used by the application for connecting and Synchronizing folders using WinSCP.
    /// </summary>
    class SFTP_Synchronize
    {
        /// <summary>
        /// Main class of FTP syncronization. Reads InputParameters text file and calls WINSCP library function for syncronization
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // get current directory
            string appPath = AppDomain.CurrentDomain.BaseDirectory;

            // Read & Store needed parameters from text file
            InputParameters Params = new InputParameters(appPath);

            // Synchronize files in Directories (local and ftp)
            new WINSCP().SynchronizeDirectories(Params);
        }
    }
}
