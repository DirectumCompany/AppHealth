using AppHealth.Configurations;
using AppHealth.Core;
using AppHealth.Tasks;
using AppHealth.Templates;
using AppHealth.Utilities;
using CLAP;
using FluentDateTime;
using FluentDateTimeOffset;
using System;
using System.IO;
using System.Linq;

namespace AppHealth
{
  class Verbs
  {

    private Verbs()
    {
      // Закрытие
    }

    /// <summary>
    /// Запуск формирования статистики для указанных конфигураций
    /// </summary>
    [Verb(IsDefault = true, Description = "Hello, Friend!")]
    public static void Hello()
    {
      var assembly = System.Reflection.Assembly.GetExecutingAssembly();
      var curVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion;

      Console.ForegroundColor = ConsoleColor.Magenta;
      Console.WriteLine("Утилита для мониторинга состояния приложений. Версия: {0}", curVersion);
      Console.ResetColor();

      var products = IISManager.GetProducts().ToArray();
      if (products.Length > 0)
      {
        Console.WriteLine("Найдены следующие приложения DIRECTUM:");
        for (var i = 0; i < products.Length; i++)
        {
          Console.WriteLine("  {0}: {1}", i, products[i]);
        }

        Console.WriteLine("\nЧтобы запустить быстрое формирование отчета по продукту воспользуйтесь командой 'start'");
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("{0} start /a:\"{1}\"", Path.GetFileName(assembly.Location), products.First());
        Console.ResetColor();
      }

      var configurations = ConfigurationManager.GetAll().ToArray();
      if (configurations.Length > 0)
      {
        Console.WriteLine("\nТак же можно использовать существующие конфигурации:");
        foreach (var configuration in configurations)
        {
          Console.WriteLine("  {0}: {1}", configuration.Name, configuration.Description);
        }

        Console.WriteLine("\nЧтобы запустить формирование отчета воспользуйтесь 'start'");
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("{0} start /c:\"{1}\"", Path.GetFileName(assembly.Location), configurations.First().Name);
        Console.ResetColor();
      }
      else
      {
        var templates = TemplateManager.GetAll().ToArray();
        Console.WriteLine("\nЧтобы создать конфигурацию по продукту воспользуйтесь командой 'create'");
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("{0} create /t:\"{1}\"", Path.GetFileName(assembly.Location), templates.First().Name);
        Console.ResetColor();

        Console.WriteLine("Доступны следующие шаблоны:");
        foreach (var template in templates)
        {
          Console.WriteLine("  {0}: {1}", template.Name, template.Description);
        }
      }


      Console.WriteLine("\nДля уточнения других параметров запуска и команд воспользуйтесь командой 'help'");
      Console.ForegroundColor = ConsoleColor.DarkCyan;
      Console.WriteLine("{0} help", Path.GetFileName(assembly.Location));
      Console.ResetColor();
    }

    /// <summary>
    /// Запуск формирования статистики для указанных конфигураций
    /// </summary>
    /// <param name="config">Имя конфигурации</param>
    /// <param name="period">Предопределенный период для анализа</param>
    /// <param name="fromDate">Дата начала периода анализа</param>
    /// <param name="toDate">Дата окончания периода анализа</param>    
    /// <param name="inclusivePeriod">Признак использования даты окончания включительно</param>
    [Verb(Description = "Запуск формирования статистики для указанных конфигураций")]
    public static void Start(
      [Description("Имя приложения для запуска анализа")]
      string application,
      [Description("Путь к файлам лога приложения")]
      string logPath,
      [DefaultValue("All")]
      [Description("Наименование конфигурации. По умолчанию обрабатываются все")]
      string[] config,
      [DefaultValue(DatePeriod.LastDay)]
      [Description("Предопределенный период обработки. По умолчанию обработка выполняется за прошлый день")]
      DatePeriod period,
      [Description("Дата начала обработки в формате \"yyyy-MM-dd\" или \"yyyy-MM-dd HH:mm:ss\"")]
      DateTime fromDate,
      [Description("Дата конца обработки в формате \"yyyy-MM-dd\" или \"yyyy-MM-dd HH:mm:ss\"")]
      DateTime toDate
      )
    {
      //TODO: Разбить на методы!!!

      // Анализ по логам
      if (!string.IsNullOrEmpty(logPath))
      {
        Configurations.Configuration configuration;

        foreach (var template in TemplateManager.GetAll().Where(t => !string.IsNullOrEmpty(t.PathModeMask)))
        {
          var files = Directory.GetFiles(logPath, template.PathModeMask);
          if (files.Length > 0)
          {
            var dates = files.Select(f => Utility.ExtractDateTime(f)).OrderBy(f => f);
            if (fromDate == DateTime.MinValue)
              fromDate = dates.FirstOrDefault();
            if (toDate == DateTime.MinValue)
              toDate = dates.LastOrDefault();
            FillPeriod(period, ref fromDate, ref toDate);

            configuration = ConfigurationManager.CreateInstant(template, logPath);
            configuration.Run(fromDate, toDate);
          }
        }
        return;
      }

      // Анализ по приложению
      if (!string.IsNullOrEmpty(application))
      {
        var product = IISManager.GetProducts().FirstOrDefault(a => a.ToString().Equals(application, StringComparison.OrdinalIgnoreCase));

        if (product == null)
        {
          Core.Application.Log(LogLevel.Error, "Приложение '{0}' не найдено.", application);
          return;
        }

        var configuration = ConfigurationManager.CreateInstant(product);
        //Заполнение дат, при указании предопределенного периода
        FillPeriod(period, ref fromDate, ref toDate);
        configuration.Run(fromDate, toDate);
        return;
      }


      if (config.Contains("All"))
        config = Directory.GetDirectories("Configurations").Select(x => Path.GetFileName(x)).ToArray();

      //Заполнение дат, при указании предопределенного периода
      FillPeriod(period, ref fromDate, ref toDate);

      foreach (var name in config)
      {
        try
        {
          Core.Application.Log(LogLevel.Informational, "Запуск обработки '{0}' c {1} до {2}, period {3}", name, fromDate, toDate, period);

          var configuration = ConfigurationManager.Get(name);
          if (configuration == null)
          {
            Core.Application.Log(LogLevel.Error, "Конфигурация '{0}' отсутствует.", name);
            Configurations();
            return;
          }

          configuration.Run(fromDate, toDate);    //Core.Configuration(configName).Run()
        }
        catch (OperationCanceledException)
        {
          Core.Application.Log(LogLevel.Informational, "Вызвано завершение обработки конфигурации {0}", name);
        }
        catch (Exception ex)
        {
          Core.Application.Log(LogLevel.Error, "Во время обработки конфигурации '{0}' возникла ошибка: {1}", name, ex.Message);
          while (ex.InnerException != null)
          {
            ex = ex.InnerException;
            Core.Application.Log(LogLevel.Error, "\t --> {0}", ex.Message);
          }
        }
      }
    }

    /// <summary>
    /// Информация по доступным задачам
    /// </summary>
    /// <param name="name">Имя задачи для получения расширенной информации</param>
    [Verb(IsDefault = false, Description = "Информация по доступным задачам")]
    public static void Tasks(
        [Description("Имя задачи для получения расширенной информации")]
        string name,
        [DefaultValue("True")]
        [Description("Вызов справки")]
        bool help)
    {
      if (!string.IsNullOrEmpty(name))
      {
        Type type;

        if (TaskFactory.TaskHandlerTypes.TryGetValue(name, out type))
        {
          Tasks.ITask task = ((Tasks.ITask)Activator.CreateInstance(type));
          Console.WriteLine(task.GetDescription());
          Console.WriteLine(task.GetHelp());
        }
        else
        {
          Console.WriteLine("Задание '{0}' не найдено.", name);
          name = null;
        }
      }

      if (string.IsNullOrEmpty(name))
      {
        Console.WriteLine("Список доступных задач:");
        foreach (var task in TaskFactory.TaskHandlerTypes)
          Console.WriteLine("    {0}: {1}.", task.Key, ((Tasks.ITask)Activator.CreateInstance(task.Value)).GetDescription().TrimEnd('.'));
      }
    }

    /// <summary>Информация по доступным конфигурациям</summary>
    [Verb(IsDefault = false, Description = "Информация по доступным конфигурациям")]
    public static void Configurations()
    {
      Console.WriteLine("Список доступных конфигураций:");
      foreach (var configuration in ConfigurationManager.GetAll())
        Console.WriteLine("    {0}: {1}.", configuration.Name, configuration.Description);
    }

    /// <summary>
    /// Информация по доступным шаблонам конфигураций
    /// </summary>
    /// <param name="name">Имя задачи для получения расширенной информации</param>
    [Verb(IsDefault = false, Description = "Информация по доступным шаблонам конфигураций")]
    public static void Templates()
    {
      Console.WriteLine("Список доступных шаблонов конфигураций:");
      foreach (var template in TemplateManager.GetAll())
      {
        Console.WriteLine("    {0}: {1}.", template.Name, template.Description);
      }
    }

    /// <summary>
    /// Создание конфигурации по шаблону
    /// </summary>
    /// <param name="name">Имя шаблона для создания конфигурации</param>
    [Verb(IsDefault = false, Description = "Создание конфигурации по шаблону")]
    public static void Create(
        [Description("Шаблон для создания конфигурации")]
        string template
      )
    {
      var templ = TemplateManager.Get(template);
      TemplateManager.CreateConfiguration(templ);
    }

    /// <summary>
    /// Вывод хелпа по пользованию утилитой (автогенерируемый)
    /// </summary>
    /// <param name="help"></param>
    [Help]
    public static void Help(string help)
    {
      var assembly = System.Reflection.Assembly.GetExecutingAssembly();
      var curVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion;
      Console.ForegroundColor = ConsoleColor.Magenta;
      Console.WriteLine("Утилита для мониторинга состояния приложений. Версия: {0}", curVersion);
      Console.ResetColor();
      Console.WriteLine(help);

      Console.WriteLine();
      Console.ForegroundColor = ConsoleColor.Magenta;
      Console.WriteLine("Предопределённые параметры, при запуске конфигураций:");
      Console.ResetColor();

      Console.WriteLine("   FromDate: Дата с которой запущено формирование отчёта");
      Console.WriteLine("   ToDate: Дата до которой запущено формирование отчёта");
      Console.WriteLine("   ConfigurationPath: Путь к папке с выполняемой конфигурацией");
      Console.WriteLine("   ReportsPath: Путь к папке с отчётами");
      Console.WriteLine();
    }

    /// <summary>
    /// Заполнение предопределенного периода
    /// </summary>
    /// <param name="period">Предопределенный период</param>
    /// <param name="fromDate">Дата начала</param>
    /// <param name="toDate">Дата конца</param>
    private static void FillPeriod(DatePeriod period, ref DateTime fromDate, ref DateTime toDate)
    {
      //Если был задан fromDate и период по-умолчанию - устанавливаем пользовательский период
      if (fromDate != DateTime.MinValue && period == DatePeriod.LastDay) period = DatePeriod.Custom;

      switch (period)
      {
        case DatePeriod.LastWeek:
          var weekEarlier = DateTime.Now.WeekEarlier().Date;
          fromDate = weekEarlier.FirstDayOfWeek();
          toDate = weekEarlier.LastDayOfWeek().AddDays(1);
          break;
        case DatePeriod.LastMonth:
          var previousMonth = DateTime.Now.PreviousMonth().Date;
          fromDate = previousMonth.FirstDayOfMonth();
          toDate = previousMonth.LastDayOfMonth().AddDays(1);
          break;
        case DatePeriod.CurrentMonth:
          fromDate = DateTime.Now.FirstDayOfMonth().Date;
          toDate = DateTime.Now.Date;
          break;
        case DatePeriod.LastDay:
          fromDate = DateTime.Now.Date.AddDays(-1);
          toDate = DateTime.Now.Date;
          break;
        case DatePeriod.CurrentDay:
          fromDate = DateTime.Now.Date;
          toDate = DateTime.Now.Date.AddDays(1);
          break;
        case DatePeriod.Custom:
          if (fromDate == DateTime.MinValue) fromDate = DateTime.Now.Date.AddDays(-1);
          if (toDate == DateTime.MinValue) toDate = DateTime.Now.Date;
          if (toDate <= fromDate) toDate = fromDate.AddDays(1); //Если равны, то продлим
          break;
      }
    }
  }
}
