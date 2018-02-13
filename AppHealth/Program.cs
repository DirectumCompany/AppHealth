using CLAP;
using AppHealth.Core;
using System;
using System.IO;

namespace AppHealth
{
  static class Program
  {
    static void Main(string[] args)
    {
      try
      {
        var workingFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        Application.Initialization(workingFolder);
        Configurations.ConfigurationManager.GetAll();
        Parser.Run<Verbs>(args);
      }
      catch (UnhandledParametersException ex)
      {
        Application.Log(LogLevel.Error, "Указаны некорректные параметры: {0}.", String.Join(", ", ex.UnhandledParameters.Keys));
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        Application.Log(LogLevel.Important, "Воспользуйтесь справкой '{0} help'.", Path.GetFileName(assembly.Location));
      }
      catch (VerbNotFoundException ex)
      {
        Application.Log(LogLevel.Error, "Указана некорректная команда: {0}.", ex.Verb);
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        Application.Log(LogLevel.Important, "Воспользуйтесь справкой '{0} help'.", Path.GetFileName(assembly.Location));
      }
      catch (MissingArgumentPrefixException ex)
      {
        Application.Log(LogLevel.Error, "Указан некорректный аргумент: {0}.", ex.ParameterName);
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        Application.Log(LogLevel.Important, "Воспользуйтесь справкой '{0} help'.", Path.GetFileName(assembly.Location));
      }
      catch (Exception ex)
      {
        Application.Log(LogLevel.Error, "Во время работы программы возникла ошибка: {0}", ex.Message);
        while (ex.InnerException != null)
        {
          ex = ex.InnerException;
          Application.Log(LogLevel.Error, "\t --> {0}", ex.Message);
        }
      }
      finally
      {
        Application.Log(LogLevel.Informational, "Завершение работы приложения.");
        Application.Exit();
      }
    }

  }
}
