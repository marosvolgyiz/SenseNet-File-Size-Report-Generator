using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SNFileSizeReportGenerator
{
    class Email
    {
        static public bool Send(string attachmentPath, string appStarted)
        {
            bool success = true;
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    
                    mail.From = new MailAddress(Config.EmailFrom);
                    mail.Subject = Config.SubjectPrefix + " " + appStarted;
                    mail.Body = "Server: " + Config.ServerName + "\n";
                    mail.Body += "Database: " + Config.DatabaseName + "\n";
                    if (Log.CheckErrorLogExists())
                    {
                        mail.Body += "Error have occured: ";
                        var errors = Log.ReturnErrorLogs();
                        mail.Body += " \n" + errors;
                    }
                    else
                    {
                        mail.Body += "The script finnished running without error.";
                    }
                   
                    foreach (var EMTo in Config.EmailList)
                    {
                        MailAddress mailTo;                
                        mailTo = new MailAddress(EMTo);
                        mail.To.Add(mailTo);
                    }
            
                    if (!string.IsNullOrEmpty( attachmentPath ) && System.IO.File.Exists(attachmentPath))
                    {                    
                        mail.Attachments.Add(new Attachment(attachmentPath));                    
                    }
                    else
                    {
                        Log.LogErrorEvent("Missing e-mail attachment.", "Missing e-mail attachment.");
                        return false;
                    }

                    SmtpClient client = new SmtpClient(Config.SMTP ?? "localhost");            
                    client.Send(mail);
                }
                return success;
            }
            catch (Exception e)
            {
                Log.LogErrorEvent(e.ToString(), "E-mail could not sent.");
                return false;
            }
                  
        }
    }
}
