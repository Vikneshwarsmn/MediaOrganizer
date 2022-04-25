using System.IO;
using System.Linq;

namespace MediaOrganizer
{
    public class MediaValidator
    {
        private static readonly string[] ValidImageExtensions = { ".jpg", ".jpeg", ".jfif", ".pjpeg", ".pjp", ".bmp", ".gif", ".png" };
        private static readonly string[] ValidVideoExtensions = { ".webm", ".mpg", ".mp2", ".mpeg", ".mpe", ".mpv", ".ogg", ".mp4", ".m4p", ".m4v", ".avi", ".wmv", ".mov", ".qt", ".flv", ".swf", ".avchd" };

        public static string GetFileMediaType(string fileToCopy)
        {
            if (IsFileImage(fileToCopy))
            {
                return FileType.Photo;
            }
            else if (IsFileVideo(fileToCopy))
            {
                return FileType.Video;
            }
            else
            {
                return FileType.None;
            }
        }

        private static bool IsFileImage(string currentFile)
        {
            string fileExtension = Path.GetExtension(currentFile);
            return ValidImageExtensions.Contains(fileExtension.ToLower());
        }

        private static bool IsFileVideo(string currentFile)
        {
            string fileExtension = Path.GetExtension(currentFile);
            return ValidVideoExtensions.Contains(fileExtension.ToLower());
        }
    }
}
