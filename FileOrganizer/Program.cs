using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FileOrganizer
{
    class Program
    {
        private static readonly bool IsCopy = false;
        private static readonly string SourcePath = @"H:\YandexDisk\My.Media.Collection\2016\12.December\Andaman & Nicobar";
        private static readonly string PhotosCollectionPath = @"F:\HoneyMoon\Photos";
        private static readonly string VideosCollectionPath = @"F:\HoneyMoon\Videos";
        private static long FILE_COUNT = 0;
        private static long FILE_COPY_COUNT = 0;
        private static long TOTAL_SCAN_SIZE = 0;
        private static readonly string[] ValidImageExtensions = { ".jpg", ".jpeg", ".jfif", ".pjpeg", ".pjp", ".bmp", ".gif", ".png" };
        private static readonly string[] ValidVideoExtensions = { ".webm", ".mpg", ".mp2", ".mpeg", ".mpe", ".mpv", ".ogg", ".mp4", ".m4p", ".m4v", ".avi", ".wmv", ".mov", ".qt", ".flv", ".swf", ".avchd" };

        static Regex r = new Regex(":");

        static void Main(string[] args)
        {
            Console.Title = "FileOrganizer";

            Console.WriteLine("Scan Path: " + SourcePath);
            Console.WriteLine("Scanning Started.");

            GetDirectoryReadyForScanning(SourcePath);

            Console.WriteLine("-------------------------------------------");
            Console.WriteLine("Scan Report");
            Console.WriteLine("Total Files Scanned: " + FILE_COUNT);
            Console.WriteLine("Total Files Copied: " + FILE_COPY_COUNT);
            Console.WriteLine("Total Data Scanned: " + GetFileSize(TOTAL_SCAN_SIZE));

            Console.ReadKey();
        }

        private static void GetDirectoryReadyForScanning(string folderPath)
        {
            // Start with drives if you have to search the entire computer.
            DirectoryInfo dirInfo = new DirectoryInfo(folderPath);

            SearchDirectoryRecursive(dirInfo);
        }

        private static void SearchDirectoryRecursive(DirectoryInfo root)
        {
            DirectoryInfo[] subDirs = null;
            FileInfo[] files = null;

            // First, process all the files directly under this folder
            if (!((root.Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint))
            {
                try
                {
                    files = root.GetFiles("*.*");

                    //var isDirectoryEmpty = !Directory.EnumerateFileSystemEntries(root.FullName).Any();

                    //if (isDirectoryEmpty)
                    //{
                    //    Directory.Delete(root.FullName, false);
                    //}
                }
                // This is thrown if even one of the files requires permissions greater
                // than the application provides.
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine("Unauthorized Access " + root.FullName);
                }
                catch (DirectoryNotFoundException e)
                {
                    Console.WriteLine("Directory Not Found " + root.FullName);
                }

                if (files != null)
                {
                    foreach (FileInfo fi in files)
                    {
                        string fileShortDescription = FileShortDescription(fi.FullName);
                        Console.WriteLine("Current File: " + fileShortDescription);
                        CheckMediaType(fi.FullName);
                        FILE_COUNT++;
                        TOTAL_SCAN_SIZE += fi.Length;
                    }
                }

                // Now find all the subdirectories under this directory.
                try
                {
                    subDirs = root.GetDirectories();
                }
                catch (UnauthorizedAccessException e)
                {

                    Console.WriteLine("Unauthorized Access " + root.FullName);
                }
                catch (DirectoryNotFoundException e)
                {
                    Console.WriteLine("Directory Not Found " + root.FullName);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Other Error " + root.FullName + e.Message);
                }

                if (subDirs != null)
                {
                    foreach (DirectoryInfo dirInfo in subDirs)
                    {
                        try
                        {
                            SearchDirectoryRecursive(dirInfo);
                        }
                        catch (PathTooLongException ex)
                        {
                            Console.WriteLine(String.Format("Path too long for file name : {0}", dirInfo.Name));
                        }
                    }
                }
            }
        }

        private static void CheckMediaType(string fileToCopy)
        {
            string collectionsPath = string.Empty;

            string mediaType = GetFileMediaType(fileToCopy);
            if (mediaType.Equals("Image"))
            {
                collectionsPath = PhotosCollectionPath;
            }
            else if (mediaType.Equals("Video"))
            {
                collectionsPath = VideosCollectionPath;
            }

            if (!string.IsNullOrEmpty(collectionsPath))
            {
                string[] ar = GetDateTakenFromImage(fileToCopy);

                DateTime lastModified = File.GetLastWriteTime(fileToCopy);
                string year = lastModified.ToString("yyyy");
                string month = lastModified.ToString("MM");
                string day = lastModified.ToString("dd");
                collectionsPath = Path.Combine(collectionsPath, year, month, day);
                //FileCopyPath(collectionsPath, fileToCopy);
            }
        }

        //retrieves the datetime WITHOUT loading the whole image
        private static string[] GetDateTakenFromImage(string path)
        {
            try
            {
                //// Create an Image object. 
                //System.Drawing.Image image = new Bitmap(@"c:\FakePhoto.jpg");

                //// Get the PropertyItems property from image.
                //PropertyItem[] propItems = image.PropertyItems;

                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    using (Image myImage = Image.FromStream(fs, false, false))
                    {
                        PropertyItem propItem = myImage.GetPropertyItem(36867);
                        string dateTaken = r.Replace(Encoding.UTF8.GetString(propItem.Value), "-", 2);
                        if (string.IsNullOrEmpty(dateTaken))
                            return null;
                        else
                            return DateTime.Parse(dateTaken).ToString("yyyy-MM-dd").Split('-');
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        private static string FileShortDescription(string fileName)
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

        private static string GetFileSize(double bytes)
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

        private static string GetFileMediaType(string fileToCopy)
        {
            if (IsFileImage(fileToCopy))
            {
                return "Image";
            }
            else if (IsFileVideo(fileToCopy))
            {
                return "Video";
            }
            else
            {
                return "None";
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

        private static void FileCopyPath(string collectionsPath, string fileToCopy)
        {
            try
            {
                if (!Directory.Exists(collectionsPath))
                    Directory.CreateDirectory(collectionsPath);

                string fileName = Path.Combine(collectionsPath, Path.GetFileName(fileToCopy));

                if (IsCopy)
                {
                    File.Copy(fileToCopy, fileName, true);
                }
                else
                {
                    if (FileEquals(fileToCopy, fileName))
                    {
                        File.Delete(fileName);
                    }
                    File.Move(fileToCopy, fileName);
                }
                FILE_COPY_COUNT++;
            }
            catch (Exception ex)
            {

            }
        }

        private static bool FileEquals(string path1, string path2)
        {
            try
            {
                byte[] file1 = File.ReadAllBytes(path1);
                byte[] file2 = File.ReadAllBytes(path2);
                if (file1.Length == file2.Length)
                {
                    for (int i = 0; i < file1.Length; i++)
                    {
                        if (file1[i] != file2[i])
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {

            }

            return false;
        }
    }
}
