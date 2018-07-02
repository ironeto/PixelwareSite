using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PixelwareSite.Extensions
{
    public static class ConfigurationExtension
    {
        public static List<T> GetListValue<T>(this IConfiguration configuration, string section)
        {
            var result = new List<T>();
            configuration.GetSection(section).Bind(result);
            return result;
        }
    }
}
