using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace MediaOrganizer
{
    public class ImagePropertyRetriever
    {
        static Regex r = new Regex(":");

        /// Retrieves the datetime WITHOUT loading the whole image
        public static FileCreatedMetadata GetImageCreatedDate(string path)
        {
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    using (Image myImage = Image.FromStream(fs, false, false))
                    {
                        PropertyItem propItem = myImage.GetPropertyItem(36867);
                        string dateTaken = r.Replace(Encoding.UTF8.GetString(propItem.Value), "-", 2);
                        if (string.IsNullOrEmpty(dateTaken))
                        {
                            return null;
                        }
                        else
                        {
                            string[] fileCreatedMetadata = DateTime.Parse(dateTaken).ToString("yyyy-MM-dd").Split('-');

                            return new FileCreatedMetadata()
                            {
                                Year = fileCreatedMetadata[0],
                                Month = fileCreatedMetadata[1],
                                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(fileCreatedMetadata[1])),
                                Day = fileCreatedMetadata[2]
                            };
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static FileCreatedMetadata GetFileCreationTime(string path)
        {
            DateTime fileCreatedDate = File.GetCreationTime(path);

            string[] fileCreatedMetadata = fileCreatedDate.ToString("yyyy-MM-dd").Split('-');

            return new FileCreatedMetadata()
            {
                Year = fileCreatedMetadata[0],
                Month = fileCreatedMetadata[1],
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(fileCreatedMetadata[1])),
                Day = fileCreatedMetadata[2]
            };
        }
    }
}
