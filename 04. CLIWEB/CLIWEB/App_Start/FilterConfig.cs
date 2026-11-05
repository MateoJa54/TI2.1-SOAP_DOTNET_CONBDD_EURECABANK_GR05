using System.Web.Mvc;
using ec.edu.monster.web.util;

namespace ec.edu.monster.web
{
    public static class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new FiltroSesion());
        }
    }
}
