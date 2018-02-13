using Microsoft.Web.Administration;

namespace AppHealth.Core
{
  class IISApplication
  {
    public Site Site { get; set; }
    public Microsoft.Web.Administration.Application Application { get; set; }
    public string Path { get; set; }
    public string IISLogPath { get; set; }
    public string Code { get; set; }

    public override string ToString()
    {
      return string.Format("{0}{1}", Site.Name, Application.Path).TrimEnd('/');
    }
  }
}
