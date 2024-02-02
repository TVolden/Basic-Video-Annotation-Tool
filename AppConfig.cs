using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace Video_Annotation
{
    internal class AppConfig
    {
        private readonly IConfiguration _configuration;

        public AppConfig() 
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Package.Current.InstalledLocation.Path)
                .AddJsonFile("appsettings.json", optional: false);

            _configuration = builder.Build();
        }

        private T GetSection<T>(string key) => _configuration.GetSection(key).Get<T>();
        public Label[] Labels => GetSection<Label[]>(nameof(Labels));
    }
}
