using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MediaOrganizer
{
    class Program
    {
        private static long FILE_COUNT = 0;
        private static long FILE_COPY_COUNT = 0;
        private static long TOTAL_SCAN_SIZE = 0;
        private static readonly string[] ValidImageExtensions = { ".jpg", ".jpeg", ".jfif", ".pjpeg", ".pjp", ".bmp", ".gif", ".png" };
        private static readonly string[] ValidVideoExtensions = { ".webm", ".mpg", ".mp2", ".mpeg", ".mpe", ".mpv", ".ogg", ".mp4", ".m4p", ".m4v", ".avi", ".wmv", ".mov", ".qt", ".flv", ".swf", ".avchd" };

        static Regex r = new Regex(":");

        static void Main(string[] args)
        {
            string photosSourcePath = Configurations.PhotosSourcePath;

            Console.Title = "FileOrganizer";

            Console.WriteLine("Scan Path: " + photosSourcePath);
            Console.WriteLine("Scanning Started.");

            GetDirectoryReadyForScanning(photosSourcePath);

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
            if (mediaType.Equals(FileType.Photo))
            {
                collectionsPath = Path.Combine(Configurations.PhotosOrganizeBasePath, FileType.Photo);

                if (!string.IsNullOrEmpty(collectionsPath))
                {
                    FileCreatedMetadata fileTakenMetadata = GetImageCreatedDate(fileToCopy);
                    collectionsPath = Path.Combine(collectionsPath, fileTakenMetadata.Year, fileTakenMetadata.Month);
                }
            }
            else if (mediaType.Equals(FileType.Video))
            {
                collectionsPath = Path.Combine(Configurations.PhotosOrganizeBasePath, FileType.Video); ;
                if (!string.IsNullOrEmpty(collectionsPath))
                {
                    DateTime lastModified = File.GetLastWriteTime(fileToCopy);
                    string year = lastModified.ToString("yyyy");
                    string month = lastModified.ToString("MM");
                    string day = lastModified.ToString("dd");
                    collectionsPath = Path.Combine(collectionsPath, year, month, day);
                }
            }

            FileCopyPath(collectionsPath, fileToCopy);
        }

        /// Retrieves the datetime WITHOUT loading the whole image
        private static FileCreatedMetadata GetImageCreatedDate(string path)
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
                            string[] takenDateMetadata = DateTime.Parse(dateTaken).ToString("yyyy-MM-dd").Split('-');

                            return new FileCreatedMetadata()
                            {
                                Year = takenDateMetadata[0],
                                Month = takenDateMetadata[1],
                                Day = takenDateMetadata[2]
                            };
                        }
                    }
                }
            }
            catch(Exception ex)
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

        private static void FileCopyPath(string collectionsPath, string fileToCopy)
        {
            try
            {
                if (!Directory.Exists(collectionsPath))
                    Directory.CreateDirectory(collectionsPath);

                string fileName = Path.Combine(collectionsPath, Path.GetFileName(fileToCopy));

                File.Copy(fileToCopy, fileName, true);

                FILE_COPY_COUNT++;
            }
            catch (Exception ex)
            {

            }
        }
    }
}
