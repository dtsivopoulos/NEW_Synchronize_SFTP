using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SFTP_Synchronize
{
    /// <summary>
    /// Sends email about the status of the current process of the application
    /// </summary>
    class SendEmail
    {
        /// <summary>
        /// Sends email based on the properties stored in InputParameters
        /// </summary>
        /// <param name="success">true if successfull, false if not</param>
        /// <param name="Parameters">Holds the needed properties</param>
        /// <param name="messages">Appends these messages into the email body</param>
        public static void Send(bool success, InputParameters Parameters, StringBuilder messages)
        {
            string message = "";

            try
            {
                string HostServer = Parameters.EmailHostServer;
                int HostPort = Parameters.EmailHostPort;
                string from_email = Parameters.EmailFrom;
                string from_password = Parameters.EmailFromPassword;
                string subject = Parameters.EmailSubject + " | " + (Parameters.UsedForDownloading.Equals("TRUE") ? "Downloading" : "Uploading") + " : " + (success ? "Success" : "Failed / Error");
                string recipient = Parameters.EmailRecipient;

                StringBuilder body = new StringBuilder((Parameters.UsedForDownloading.Equals("TRUE") ? "Downloading" : "Uploading") + " Process. ");
                body.AppendLine(" ");
                body.Append(messages);
                
                SmtpClient client = new SmtpClient
                {
                    Host = HostServer,
                    Port = HostPort,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(from_email, from_password),
                    Timeout = 20000
                };

                // construct mail message and send it
                MailMessage mm = new MailMessage(from_email, recipient, subject, body.ToString());
                client.Send(mm);

                message = "Email Sent to " + recipient + " with subject "+ subject;
                Console.WriteLine(message);
                EventLogging.WriteEntry(message, EventLogEntryType.SuccessAudit);
            }
            catch (Exception e)
            {
                message = "Could not send email\n\n" + e.ToString();
                Console.WriteLine(message);
                EventLogging.WriteEntry(message, EventLogEntryType.Error);
            }
        }
    }
}
