using AppHealth.Properties;
using System.Globalization;
using System.Net.Mail;

namespace AppHealth.Core
{
  class CrashNotifier : ILogger
  {

    private readonly System.Text.StringBuilder _buffer;
    private bool _isCrashed = false;

    /// <summary>
    /// Инициализация.
    /// </summary>
    public CrashNotifier()
    {
      _buffer = new System.Text.StringBuilder();
    }

    public void Log(LogLevel level, string message)
    {
      if (level >= LogLevel.Error) _isCrashed = true;

      //Немного красоты в выводе
      switch (level)
      {
        case LogLevel.Debug:
          _buffer.Append("<p style='color:Green'>");
          break;
        case LogLevel.Informational:
          _buffer.Append("<p style='color:Cyan'>");
          break;
        case LogLevel.Warning:
          _buffer.Append("<p style='color:Yellow'>");
          break;
        case LogLevel.Error:
          _buffer.Append("<p style='color:Red'>");
          break;
        case LogLevel.Important:
          _buffer.Append("<p style='color:Magenta'>");
          break;
      }
      _buffer.Append(message).Append("</p>");
    }

    public void Log(LogLevel level, string message, params object[] arg)
    {
      if (level >= LogLevel.Error) _isCrashed = true;

      //Немного красоты в выводе
      switch (level)
      {
        case LogLevel.Debug:
          _buffer.Append("<p style='color:Green'>");
          break;
        case LogLevel.Informational:
          _buffer.Append("<p style='color:Cyan'>");
          break;
        case LogLevel.Warning:
          _buffer.Append("<p style='color:Yellow'>");
          break;
        case LogLevel.Error:
          _buffer.Append("<p style='color:Red'>");
          break;
        case LogLevel.Important:
          _buffer.Append("<p style='color:Magenta'>");
          break;
      }
      _buffer.AppendFormat(message, arg).Append("</p>");
    }

    public void Flush()
    {
      if (!(Settings.Default.SendCrashReports && _isCrashed)) return;

      using (SmtpClient SmtpServer = new SmtpClient(Settings.Default.MailServer, Settings.Default.MailPort))
      {
        using (MailMessage mail = new MailMessage())
        {
          mail.From = new MailAddress(Settings.Default.MailFrom);
          foreach (var mailAddress in Settings.Default.MailTo.Split(';'))
          {
            if (!string.IsNullOrWhiteSpace(mailAddress)) mail.To.Add(mailAddress);
          }

          mail.Subject = "Ошибка запуска утилиты мониторинга";
          mail.Body = _buffer.ToString();
          mail.IsBodyHtml = true;

          SmtpServer.EnableSsl = Settings.Default.MailUseSSL;
          if (SmtpServer.Host.ToLower(CultureInfo.InvariantCulture).Contains("gmail.com")) SmtpServer.TargetName = "STARTTLS/smtp.gmail.com";
          SmtpServer.Credentials = new System.Net.NetworkCredential(Settings.Default.MailUser, Settings.Default.MailPassword);

          SmtpServer.Send(mail);
        }
      }
    }
  }
}

