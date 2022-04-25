using System;
using System.Diagnostics;
using System.IO;

namespace MediaOrganizer
{
    class Program
    {
        private static long FILE_COUNT = 0;
        private static long FILE_COPY_COUNT = 0;
        private static long TOTAL_SCAN_SIZE = 0;

        static void Main()
        {
            string photosSourcePath = Configurations.PhotosSourcePath;

            Console.Title = "FileOrganizer";

            Console.WriteLine("Scan Path: " + photosSourcePath);
            Console.WriteLine("Scanning Started.");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            GetDirectoryReadyForScanning(photosSourcePath);
            stopwatch.Stop();

            TimeSpan timespan = TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds);
            string totalTimeElapsed = string.Format("{0:D2}hour(s) {1:D2}minute(s) {2:D2}second(s)",
                                        timespan.Hours,
                                        timespan.Minutes,
                                        timespan.Seconds);

            Console.WriteLine("-------------------------------------------");
            Console.WriteLine("Scan Report");
            Console.WriteLine("Total Files Scanned: " + FILE_COUNT);
            Console.WriteLine("Total Files Copied: " + FILE_COPY_COUNT);
            Console.WriteLine("Total Data Scanned: " + FileProperties.GetFileSize(TOTAL_SCAN_SIZE));
            Console.WriteLine("Total Time Taken: " + totalTimeElapsed);

            Console.ReadKey();
        }

        #region Private Methods
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
                        string fileShortDescription = FileProperties.FileShortDescription(fi.FullName);
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

            string mediaType = MediaValidator.GetFileMediaType(fileToCopy);
            if (mediaType.Equals(FileType.Photo))
            {
                collectionsPath = Path.Combine(Configurations.PhotosOrganizeBasePath, FileType.Photo);

                if (!string.IsNullOrEmpty(collectionsPath))
                {
                    FileCreatedMetadata fileCreatedMetadata = ImagePropertyRetriever.GetImageCreatedDate(fileToCopy);

                    if (fileCreatedMetadata == null)
                    {
                        fileCreatedMetadata = ImagePropertyRetriever.GetFileLastWriteTime(fileToCopy);
                    }
                    collectionsPath = Path.Combine(collectionsPath, fileCreatedMetadata.Year, fileCreatedMetadata.Month + "." + fileCreatedMetadata.MonthName);
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
        #endregion
    }
}
