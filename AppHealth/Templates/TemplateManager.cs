using AppHealth.Configurations;
using AppHealth.Core;
using AppHealth.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace AppHealth.Templates
{
  static class TemplateManager
  {
    /// <summary>
    /// Путь к папке с шаблонами конфигураций
    /// </summary>
    internal static string TemplatesPath
    {
      get
      {
        return Path.Combine(Core.Application.WorkingFolder, "Templates");
      }
    }

    /// <summary>
    /// Создание шаблона
    /// </summary>
    /// <param name="name">Имя</param>
    /// <returns></returns>
    static public Template Get(string name)
    {
      string description = string.Empty;

      var configurationPath = Path.Combine(TemplatesPath, name, "application.config");
      var xml = XDocument.Load(configurationPath);

      var descAttr = xml.XPathSelectElement("Configuration").Attribute("description");
      var pathModeMask = xml.XPathSelectElement("Configuration").Attribute("pathModeMask")?.Value;
      var codes = xml.XPathSelectElement("Configuration")?.Attribute("products")?.Value;
      if (descAttr != null) description = descAttr.Value;


      var p = xml.XPathSelectElement(@"Configuration/Parameters");
      var serializer = new XmlSerializer(typeof(Parameters.ParameterCollection));
      var parameterCollection = (Parameters.ParameterCollection)serializer.Deserialize(p.CreateReader());

      return new Template(name, codes, description, parameterCollection.Parameters, configurationPath, pathModeMask);
    }

    /// <summary>
    /// Получение списка шаблонов
    /// </summary>
    /// <returns></returns>
    static public IEnumerable<Template> GetAll()
    {
      return Directory.GetDirectories(TemplatesPath).Select(c => Get(Path.GetFileName(c)));
    }

    /// <summary>
    /// Создание конфигурации из шаблона
    /// </summary>
    /// <returns></returns>
    static public void CreateConfiguration(Template template)
    {
      Core.Application.Log(LogLevel.Informational, template.Description);
      Core.Application.Log(LogLevel.Informational, "Поиск подходящих продуктов для конфигурации \"{0}\"", template.Name);

      try
      {
        IISApplication currentApp = null;
        var sites = IISManager.GetProducts().Where(a => template.ProductCodes.Split(';').Contains(a.Code)).ToList();
        if (sites.Count == 0) Core.Application.Log(LogLevel.Informational, "Не найдены установленные продукты для данной конфигурации");
        if (sites.Count == 1) currentApp = sites.First();

        if (sites.Count > 1)
        {
          Core.Application.Log(LogLevel.Informational, "Найдено несколько установок. Укажите соответствующий индекс продукта:");

          while (currentApp == null)
          {
            for (var i = 0; i < sites.Count; i++)
              Console.WriteLine("{0}: {1}", i, sites[i]);
            try
            {
              var index = int.Parse(Console.ReadLine());
              currentApp = sites[index];
            }
            catch
            {
              Core.Application.Log(LogLevel.Error, "Указан некорректный индекс, повторите попытку:");
            }
          }
        }
        if (currentApp != null)
        {
          Core.Application.Log(LogLevel.Informational, "Загрузка параметров для сайта \"{0}\"", currentApp);
          template.Parameters.First(x => x.Name == "ApplicationPath").Value = currentApp.Path;
          template.Parameters.First(x => x.Name == "SiteLogFile").Value = currentApp.IISLogPath;
          template.Parameters.First(x => x.Name == "Title").Value = currentApp.ToString();
        }
        else
          Core.Application.Log(LogLevel.Warning, "Некоторые параметры необходимо указать вручную.");

      }
      catch (Exception e)
      {
        Core.Application.Log(LogLevel.Error, "Произошла ошибка: {0}", e.Message);
        Core.Application.Log(LogLevel.Warning, "Некоторые параметры необходимо указать вручную.");
      }

      foreach (var parameter in template.Parameters)
      {
        do
        {
          Core.Application.Log(LogLevel.Informational, "{0}", parameter.Description);
          Console.ForegroundColor = ConsoleColor.DarkGray;

          foreach (var param in template.Parameters)
            parameter.Value = parameter.Value.Replace(string.Format("%{0}%", param.Name), param.Value);

          Console.Write("{0} ", parameter.Value);
          Console.ResetColor();
          Console.SetCursorPosition(0, Console.CursorTop);
          var value = Console.ReadLine();

          if (!string.IsNullOrEmpty(value))
            parameter.Value = value;

        } while (parameter.Required && string.IsNullOrEmpty(parameter.Value));
      }

      var configName = template.Parameters.First(x => x.Name == "Title").Value;
      configName = configName?.Replace(" ", "_").Replace("/", "_");
      while (string.IsNullOrEmpty(configName) || ConfigurationManager.Get(configName) != null)
      {
        Core.Application.Log(LogLevel.Warning, "Конфигурация с именем \"{0}\" уже существует. Укажите другое имя", configName);
        configName = Console.ReadLine();
        configName = configName?.Replace(" ", "_").Replace("/", "_");
      }

      Core.Application.Log(LogLevel.Informational, "Создание конфигурации \"{0}\"", configName);
      new DirectoryInfo(Path.Combine(TemplatesPath, template.Name)).CopyTo(Path.Combine(ConfigurationManager.ConfigurationPath, configName), true);

      var doc = new XDocument();
      var serializer = new XmlSerializer(typeof(Parameters.ParameterCollection));
      XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
      ns.Add("", "");
      using (var writer = doc.CreateWriter())
        serializer.Serialize(writer, new Parameters.ParameterCollection() { Parameters = template.Parameters.ToList() }, ns);

      var configPath = Path.Combine(ConfigurationManager.ConfigurationPath, configName, "application.config");
      var xml = XDocument.Load(configPath);
      xml.XPathSelectElement(@"Configuration/Parameters").Remove();
      var p = xml.XPathSelectElement(@"Configuration");
      p.AddFirst(doc.Root);
      xml.Save(configPath);
      Core.Application.Log(LogLevel.Informational, "Конфигурация {0} успешно создана", configName);
    }

    /// <summary>
    /// Получение шаблона по коду продукта
    /// </summary>
    /// <param name="siteCode">Код продукта</param>
    /// <returns>Шаблон</returns>
    internal static Template GetByProductCode(string siteCode, FilterMode mode)
    {
      return GetAll().FirstOrDefault(t => t.ProductCodes.ToUpperInvariant().Split(';').Contains(siteCode.ToUpperInvariant()));
    }
  }

  enum FilterMode
  {
    All = 0,
    InstantOnly = 1
  }
}
