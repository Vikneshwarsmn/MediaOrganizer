using System;
using System.Configuration;

namespace FileOrganizer
{
    public class Configurations
    {
        public static string PhotosSourcePath = ConfigurationManager.AppSettings["PhotosSourcePath"].ToString();
        public static string PhotosOrganizeBasePath = ConfigurationManager.AppSettings["PhotosOrganizeBasePath"].ToString();
        public static bool IsCopyEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["IsCopyEnabled"].ToString());
    }
}
