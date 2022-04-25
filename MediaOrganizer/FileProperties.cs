namespace MediaOrganizer
{
    public class FileProperties
    {
        public static string FileShortDescription(string fileName)
        {
            string fileShortDescription = fileName;
            if (fileName.Length > 50)
            {
                string firstTenCharacters = fileName.Substring(0, 24);
                string lastTenCharacters = fileName.Substring(fileName.Length - 24, 24);
                fileShortDescription = firstTenCharacters + "......" + lastTenCharacters;
            }
            return fileShortDescription;
        }

        public static string GetFileSize(double bytes)
        {
            string size = "0 Bytes";
            if (bytes >= 1073741824.0)
                size = string.Format("{0:##.##}", bytes / 1073741824.0) + " GB";
            else if (bytes >= 1048576.0)
                size = string.Format("{0:##.##}", bytes / 1048576.0) + " MB";
            else if (bytes >= 1024.0)
                size = string.Format("{0:##.##}", bytes / 1024.0) + " KB";
            else if (bytes > 0 && bytes < 1024.0)
                size = bytes.ToString() + " Bytes";

            return size;
        }
    }
}
