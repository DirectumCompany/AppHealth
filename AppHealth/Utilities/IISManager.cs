using AppHealth.Core;
using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace AppHealth.Utilities
{
  static class IISManager
  {
    /// <summary>
    /// Получить сайт, соответствующий коду продукта
    /// </summary>
    public static IEnumerable<IISApplication> GetProducts()
    {
      SiteCollection sites = null;

      try
      {
        var iisManager = new ServerManager();
        sites = iisManager.Sites;
      }
      catch (Exception e)
      {
        Core.Application.Log(LogLevel.Error, e.Message);
        yield break;
      }

      foreach (var site in sites)
      {
        foreach (var application in site.Applications)
        {
          foreach (var virtualDirectory in application.VirtualDirectories)
          {
            var path = Environment.ExpandEnvironmentVariables(virtualDirectory.PhysicalPath);
            string[] configs;
            try
            {
              configs = Directory.GetFiles(path, "*.config");
            }
            catch (Exception)
            {
              continue;
            }

            foreach (var config in configs)
            {
              var xml = XDocument.Load(config);
              var productCode = xml.Root.Element("NpoComputer.Product")?.Element("Code")?.Value;
              if (!string.IsNullOrEmpty(productCode))
              {
                yield return new IISApplication()
                {
                  Application = application,
                  Code = productCode,
                  Site = site,
                  Path = path,
                  IISLogPath = Environment.ExpandEnvironmentVariables(Path.Combine(site.LogFile.Directory, "W3SVC" + site.Id))
                };
                break;
              }
            }
          }
        }
      }
    }
  }
}
