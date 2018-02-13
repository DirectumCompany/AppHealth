using System;
using System.Text.RegularExpressions;

namespace AppHealth.Utilities
{
  static class Utility
  {
    public static DateTime ExtractDateTime(string fileName)
    {
      Regex regex = new Regex(@"\d{4}-\d{2}-\d{2}");
      var match = regex.Match(fileName);
      return DateTime.ParseExact(match.Value, "yyyy-MM-dd", null);
    }
  }
}
