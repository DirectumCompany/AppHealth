using AppHealth.Core;
using AppHealth.Templates;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace AppHealth.Configurations
{
  /// <summary>
  /// Класс для работы с конфигурациями
  /// </summary>
  static class ConfigurationManager
  {
    /// <summary>
    /// Путь к папке с конфигурациями
    /// </summary>
    internal static string ConfigurationPath
    {
      get
      {
        return Path.Combine(Core.Application.WorkingFolder, "Configurations");
      }
    }

    /// <summary>
    /// Создание конфигурации
    /// </summary>
    /// <param name="name">Имя</param>
    /// <returns></returns>
    static public Configuration Get(string name)
    {
      string description = string.Empty;
      Configuration configuration;

      var path = Path.Combine(ConfigurationPath, name, "application.config");
      if (!File.Exists(path)) return null;

      var xml = XDocument.Load(path);

      var configurationNode = xml.XPathSelectElement("Configuration");
      description = configurationNode?.Attribute("description")?.Value;

      var template = configurationNode.Attribute("template");
      if (template != null && !string.IsNullOrEmpty(template.Value))
      {
        configuration = CreateInstant(TemplateManager.Get(template.Value));

        foreach (var p in xml.XPathSelectElements(@"Configuration/Parameters/Parameter"))
          configuration.Parameters.AddParameter(p.Attribute("name").Value, p.Attribute("value").Value);
      }
      else
      {
        var p = xml.XPathSelectElements(@"Configuration/Parameters/Parameter");
        var parameters = new ParameterProvider(p.ToDictionary(k => k.Attribute("name").Value, v => v.Attribute("value").Value));
        parameters.AddParameter("ConfigurationPath", Path.Combine("Configurations", name));
        parameters.AddParameter("ReportsPath", Path.Combine(Core.Application.WorkingFolder, "Reports"));

        var taskNodes = xml.XPathSelectElements(@"Configuration/Tasks/Task");
        var tasks = taskNodes.Select(t => Tasks.TaskFactory.Create(t)).ToList();

        configuration = new Configuration(name, tasks, parameters, description);
      }

      return configuration;
    }

    /// <summary>
    /// Получение списка конфигураций
    /// </summary>
    /// <returns></returns>
    static public IEnumerable<Configuration> GetAll()
    {
      return Directory.GetDirectories(ConfigurationPath).Select(c => Get(Path.GetFileName(c)));
    }

    /// <summary>
    /// Создание временной конфигурации для быстрого анализа.
    /// </summary>
    /// <param name="template">шаблон</param>
    /// <param name="product">приложение</param>
    /// <returns></returns>
    internal static Configuration CreateInstant(IISApplication product)
    {
      Template template = null;
      string productCode = null;

      foreach (var config in Directory.GetFiles(product.Path, "*.config"))
      {
        var appConfig = XDocument.Load(config);
        productCode = appConfig.Root.Element("NpoComputer.Product")?.Element("Code")?.Value;
        if (string.IsNullOrEmpty(productCode)) continue;
        template = TemplateManager.GetByProductCode(productCode, FilterMode.InstantOnly);
        if (template != null) break;
      }

      if (template == null) throw new FileNotFoundException("Не найдено подходящих шаблонов для данного приложения.");

      Core.Application.Log(LogLevel.Informational, "Загрузка параметров для сайта \"{0}\"", product);

      var xml = XDocument.Load(template.ConfigurationPath);
      var p = xml.XPathSelectElements(@"Configuration/Parameters/Parameter");
      var parameters = new ParameterProvider(p.ToDictionary(k => k.Attribute("name").Value, v => v.Attribute("value").Value));
      parameters.AddParameter("ConfigurationPath", Path.Combine("Templates", template.Name));
      parameters.AddParameter("ReportsPath", Path.Combine(Core.Application.WorkingFolder, "Reports"));
      parameters.AddParameter("Title", product.ToString());
      parameters.AddParameter("SiteLogFile", product.IISLogPath);
      parameters.AddParameter("ApplicationPath", product.Path);

      if (productCode.Contains("NOMAD")) //TODO: Убрать костыль
      {
        var appPath = product.Path;
        var logSettings = XDocument.Load(Path.Combine(appPath, "LogSettings.config"));
        var path = ((IEnumerable)logSettings.XPathEvaluate("/*[name()='nlog']/*[name()='variable' and @name='logs-path']/@value")).Cast<XAttribute>().FirstOrDefault()?.Value;
        if (path != null && path.Contains("basedir")) path = Path.Combine(appPath, @"App_Data\Logs\");
        parameters.AddParameter("ApplicationLogsPath", path);
        //TODO: Получать путь
        parameters.AddParameter("ClientsLogPath", Path.Combine(appPath, @"\App_Data\ClientLogs\"));
      }

      var taskNodes = xml.XPathSelectElements(@"Configuration/Tasks/Task");
      var tasks = taskNodes.Select(t => Tasks.TaskFactory.Create(t)).ToList();

      return new Configuration(template.Name, tasks, parameters, null);
    }

    /// <summary>
    /// Временная конфигурация по пути и шаблону.
    /// </summary>
    /// <param name="template">Шаблон</param>
    /// <param name="logsPath">путь к логам</param>
    /// <returns></returns>
    internal static Configuration CreateInstant(Template template, string logsPath)
    {

      Core.Application.Log(LogLevel.Informational, "Загрузка параметров для \"{0}\"", template.Name);

      var xml = XDocument.Load(template.ConfigurationPath);
      var p = xml.XPathSelectElements(@"Configuration/Parameters/Parameter");
      var parameters = new ParameterProvider(p.ToDictionary(k => k.Attribute("name").Value, v => v.Attribute("value").Value));
      parameters.AddParameter("ConfigurationPath", Path.Combine("Templates", template.Name));
      parameters.AddParameter("ReportsPath", Path.Combine(Core.Application.WorkingFolder, "Reports"));
      parameters.AddParameter("Title", template.Name);
      parameters.AddParameter("ApplicationLogsPath", logsPath);

      var taskNodes = xml.XPathSelectElements(@"Configuration/Tasks/Task");
      var tasks = taskNodes.Select(t => Tasks.TaskFactory.Create(t)).ToList();

      return new Configuration(template.Name, tasks, parameters, null);
    }

    /// <summary>
    /// Временная конфигурация по шаблону.
    /// </summary>
    /// <param name="template">Шаблон</param>
    /// <returns></returns>
    internal static Configuration CreateInstant(Template template)
    {

      Core.Application.Log(LogLevel.Informational, "Загрузка параметров для \"{0}\"", template.Name);

      var xml = XDocument.Load(template.ConfigurationPath);
      var p = xml.XPathSelectElements(@"Configuration/Parameters/Parameter");
      var parameters = new ParameterProvider(p.ToDictionary(k => k.Attribute("name").Value, v => v.Attribute("value").Value));
      parameters.AddParameter("ConfigurationPath", Path.Combine("Templates", template.Name));
      parameters.AddParameter("ReportsPath", Path.Combine(Core.Application.WorkingFolder, "Reports"));
      parameters.AddParameter("Title", template.Name);
      var taskNodes = xml.XPathSelectElements(@"Configuration/Tasks/Task");
      var tasks = taskNodes.Select(t => Tasks.TaskFactory.Create(t)).ToList();

      return new Configuration(template.Name, tasks, parameters, null);
    }
  }
}
