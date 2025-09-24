using BetterWidgets.Consts;
using BetterWidgets.Model;
using BetterWidgets.Model.Weather;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace BetterWidgets.Properties
{
    public class Configuration
    {
        public Configuration()
        {
            Root = ConfigureRoot();
        }

        #region Props

        public IConfigurationRoot Root { get; set; }

        #endregion

        #region Utils

        private IConfigurationRoot ConfigureRoot() 
                 => new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile(FileNames.AppSettings)
                .Build();

        #endregion

        #region Values

        public string ProductVersion => Root["Product:Version"];

        public string ProductBuild => Root["Product:Build"];

        public string ProductChanel => Root["Product:Chanel"];

        public string ProductDeveloper => Root["Product:Developer"];

        public string StoreId => Root["Product:StoreId"];

        public string StorePfn => Root["Product:StorePfn"];

        public IEnumerable<CultureInfo> Languages => 
            Root.GetSection("Languages").Get<string[]>().Select(c => new CultureInfo(c));

        public MSGraphConfig MSGraph => Root.GetSection("MSGraph")?.Get<MSGraphConfig>();

        public WeatherWidgetConfig WeatherWidgetConfig => Root.GetSection("Widgets:Weather")?.Get<WeatherWidgetConfig>();

        #endregion
    }
}
