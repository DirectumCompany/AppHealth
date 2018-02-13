using AppHealth.Core;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Mail;

namespace AppHealth.Tasks
{
  /// <summary>
  /// Формирование и отправка письма
  /// </summary>
  class Mail : ITask
  {
    /// <summary>Адрес получателя</summary>
    private string _to;
    /// <summary>Адрес копий</summary>
    private string _cc;
    /// <summary>Текст сообщения</summary>
    private string _body;
    /// <summary>Имена вложений</summary>
    private string _attachment;
    /// <summary>Адрес почтового сервера</summary>
    private string _server;
    /// <summary>Порт почтового сервера</summary>
    private string _port;
    /// <summary>Пользователь аутентификации</summary>
    private string _user;
    /// <summary>Пароль пользователя аутентификации</summary>
    private string _password;
    /// <summary>email отправителя</summary>
    private string _from;
    /// <summary>Тема сообщения</summary>
    private string _subject;
    /// <summary>Признак использования SSL</summary>
    private string _useSSL;

    /// <summary>
    /// Создание задачи из XML-определения
    /// </summary>
    /// <param name="declaration">XML-определение</param>
    /// <returns></returns>
    public ITask Parse(System.Xml.Linq.XElement declaration)
    {
      if (declaration == null) throw new ArgumentNullException("Отсутствует определение задачи");

      _from = declaration.Attribute("from").Value;
      _to = declaration.Attribute("to").Value;

      if (declaration.Attribute("copy") != null)
      {
        _cc = declaration.Attribute("copy").Value;
      }

      _subject = declaration.Attribute("subject").Value;
      _body = declaration.Attribute("body").Value;

      _server = declaration.Attribute("server").Value;
      _port = declaration.Attribute("port").Value;

      if (declaration.Attribute("user") != null)
      {
        _user = declaration.Attribute("user").Value;
        _password = declaration.Attribute("password").Value;
      }

      if (declaration.Attribute("useSSL") != null)
      {
        _useSSL = declaration.Attribute("useSSL").Value;
      }

      if (declaration.Attribute("attachment") != null)
      {
        _attachment = declaration.Attribute("attachment").Value;
      }
      return this;
    }


    /// <summary>
    /// Выполнение задачи
    /// </summary>
    /// <param name="parameters">Провайдер параметров</param>
    public void Run(ParameterProvider parameters)
    {
      using (SmtpClient SmtpServer = new SmtpClient(parameters.Parse(_server).First(), int.Parse(parameters.Parse(_port).First())))
      {
        using (MailMessage mail = new MailMessage())
        {
          mail.From = new MailAddress(parameters.Parse(_from).First());
          foreach (var mailAddress in parameters.Parse(_to).First().Split(';'))
          {
            if (!string.IsNullOrWhiteSpace(mailAddress)) mail.To.Add(mailAddress);
          }

          if (_cc != null)
          {
            var cc = parameters.Parse(_cc).First();
            if (!string.IsNullOrWhiteSpace(cc))
            {
              foreach (var mailAddress in cc.Split(';'))
              {
                if (!string.IsNullOrWhiteSpace(mailAddress)) mail.CC.Add(mailAddress);
              }
            }
          }

          mail.Subject = parameters.Parse(_subject).First();
          mail.Body = parameters.Parse(_body).First();
          mail.IsBodyHtml = true;

          // Добавление вложений
          if (!string.IsNullOrEmpty(_attachment))
          {
            System.Net.Mail.Attachment attachment;
            attachment = new System.Net.Mail.Attachment(parameters.Parse(_attachment).First());
            mail.Attachments.Add(attachment);
          }

          SmtpServer.EnableSsl = Boolean.Parse(parameters.Parse(_useSSL).First());
          SmtpServer.UseDefaultCredentials = false;
          SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;
          if (_server.ToLower(CultureInfo.InvariantCulture).Contains("gmail.com")) SmtpServer.TargetName = "STARTTLS/smtp.gmail.com";
          SmtpServer.Credentials = new System.Net.NetworkCredential(parameters.Parse(_user).First(), parameters.Parse(_password).First());

          SmtpServer.Send(mail);
        }
      }
    }


    public string GetDescription()
    {
      return "Формирование и отправка письма.";
    }

    public string GetHelp()
    {
      throw new NotImplementedException();
    }
  }
}
