using AppHealth.Core;
using FuzzyString;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace AppHealth.Tasks
{
  /// <summary>
  /// Вероятностное сравнение строк
  /// </summary>
  class FuzzyCompare : ITask
  {
    /// <summary>
    /// Путь до файла с TOP ошибок
    /// </summary>
    private string _topPath;
    /// <summary>
    /// Кодировка файла
    /// </summary>
    private string _encoding;

    /// <summary>
    /// Создание задачи из XML-определения
    /// </summary>
    /// <param name="declaration">XML-определение</param>
    /// <returns></returns>
    public ITask Parse(System.Xml.Linq.XElement declaration)
    {
      if (declaration == null) throw new ArgumentNullException("Отсутствует определение задачи");
      _topPath = declaration.Attribute("path").Value;
      _encoding = declaration.Attribute("encoding") == null ? null : declaration.Attribute("encoding").Value.ToLower(CultureInfo.InvariantCulture);
      return this;
    }

    /// <summary>
    /// Выполнение задачи
    /// </summary>
    /// <param name="parameters">Провайдер параметров</param>
    public void Run(ParameterProvider parameters)
    {
      var report = new List<ReportItem>();
      var reportPath = parameters.Parse(_topPath).First();

      //Прекратить обработку, если файл не найден
      if (!File.Exists(reportPath)) return;

      var enc = String.IsNullOrEmpty(_encoding) ? System.Text.Encoding.UTF8 : System.Text.Encoding.GetEncoding(_encoding);
      using (var reader = new StreamReader(reportPath, enc))
      {
        string line = null;
        while ((line = reader.ReadLine()) != null)
        {
          var d = line.Split('\t');
          var hits = 0;
          if (d.Length > 1 && !string.IsNullOrEmpty(d[0]) && int.TryParse(d[1], out hits))
            report.Add(new ReportItem { Message = d[0], Hits = hits });
        }
      }

      // Настройка параметров сравнения. Определить эмпирически
      List<FuzzyStringComparisonOptions> options = new List<FuzzyStringComparisonOptions>();
      options.Add(FuzzyStringComparisonOptions.UseHammingDistance);
      options.Add(FuzzyStringComparisonOptions.UseJaccardDistance);
      options.Add(FuzzyStringComparisonOptions.UseLongestCommonSubsequence);
      options.Add(FuzzyStringComparisonOptions.UseLongestCommonSubstring);
      options.Add(FuzzyStringComparisonOptions.UseOverlapCoefficient);
      options.Add(FuzzyStringComparisonOptions.UseRatcliffObershelpSimilarity);
      options.Add(FuzzyStringComparisonOptions.UseSorensenDiceDistance);
      options.Add(FuzzyStringComparisonOptions.UseTanimotoCoefficient);
      options.Add(FuzzyStringComparisonOptions.CaseSensitive);

      // Агрегация списка
      for (var i = 0; i < report.Count; i++)
      {
        var currentItem = report[i];
        var j = i + 1;
        while (j < report.Count)
        {
          var lookupItem = report[j];
          // Вероятностное сравнение
          if (currentItem.Message.ApproximatelyEquals(lookupItem.Message, options, FuzzyStringComparisonTolerance.Normal))
          {
            currentItem.Hits = currentItem.Hits + lookupItem.Hits;
            report.RemoveAt(j);
          }
          else
            j = j + 1;
        }
      }

      // Запись итогового списка //TODO:ТОП и путь в параметры)
      File.WriteAllLines(reportPath, report.OrderByDescending(x => x.Hits).Take(20).Select(x => string.Format("{0}\t{1}", x.Message, x.Hits)));
    }

    // Класс-элемента отчета
    private class ReportItem
    {
      public string Message { get; set; }
      public int Hits { get; set; }
    }



    public string GetDescription()
    {
      return "Вероятностное сравнение строк";
    }

    public string GetHelp()
    {
      throw new NotImplementedException();
    }
  }


}
