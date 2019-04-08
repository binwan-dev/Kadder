using System;

namespace Atlantis.Grpc.Logging.Providers
{
    public class ProviderSetting
    {
        public const string FileNameDateSymbol="{Date}";

        public static ProviderSetting Default{get;private set;}

        public string FilePath{get;set;}
        
        public string FileName{get;set;}

        public bool IsRollingFile{get;set;}

        /// <summary>
        /// useless properties
        /// </summary>
        [Obsolete("The template is not support!")]
        public string Template{get;set;}
        
        public string Other{get;set;}

        public static void SetDefault(ProviderSetting setting)
        {
            Default=setting??throw new ArgumentNullException("The logger provider setting is null, default provider setting set failed!");
        }
 
   }

    
}
