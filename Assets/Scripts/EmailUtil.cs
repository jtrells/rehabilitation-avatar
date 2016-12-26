using UnityEngine;
using System.Collections;
using System.Net.Mail;

public static class EmailUtil {

    public static void SendMail(string filePath) {
        MailMessage mail = new MailMessage("rehabilitation-avatar@mail.com", "mastercava@hotmail.it");
        SmtpClient client = new SmtpClient();
        Attachment attachment = new Attachment(filePath);
        mail.Attachments.Add(attachment);
        client.Port = 587;
        client.DeliveryMethod = SmtpDeliveryMethod.Network;
        client.UseDefaultCredentials = false;
        client.Host = "smtp.mail.com";
        client.Credentials = (System.Net.ICredentialsByHost)new System.Net.NetworkCredential("rehabilitation-avatar@mail.com", "password");
        mail.Subject = "New training log for patient " + PlayerPrefs.GetString("PatientId");
        mail.Body = "The training log file is attached to this email.";
        client.Send(mail);
    }
}
