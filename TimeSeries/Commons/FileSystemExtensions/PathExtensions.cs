using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using log4net;

namespace SKBKontur.Catalogue.FileSystemExtensions
{
    public static class PathExtensions
    {
        public static void CreateDirectoryIfNotExists(string path)
        {
            if(!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public static void MakeEmptyDirectory(string path)
        {
            CreateDirectoryIfNotExists(path);
            var directoryInfo = new DirectoryInfo(path);
            foreach(var file in directoryInfo.GetFiles())
                file.Delete();
            foreach(var dir in directoryInfo.GetDirectories())
                dir.Delete(true);
        }

        public static void DeleteDirectoryWithRetriesAsync(string path)
        {
            Task.Factory.StartNew(() =>
                {
                    try
                    {
                        logger.InfoFormat("Start delete directory '{0}'", path);
                        DeleteDirectoryWithRetries(path);
                        logger.InfoFormat("Finish delete directory '{0}'", path);
                    }
                    catch(Exception e)
                    {
                        logger.Error(string.Format("Error while delete directory '{0}'.", path), e);
                    }
                });
        }

        public static void CreateAllDirectoriesAndWriteFile(string path, byte[] content)
        {
            var fileName = path.Split('\\').Last();
            var directoryName = path.Substring(0, path.Length - fileName.Length);
            if(!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);
            File.WriteAllBytes(path, content);
        }

        public static FileInfo[] GetAllFiles(string path)
        {
            path = path.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
            var result = files.Select(fileName =>
                {
                    var pathToFile = Path.Combine(path, fileName);
                    return new FileInfo
                        {
                            Name = pathToFile.Substring(path.Length),
                            Content = File.ReadAllBytes(pathToFile)
                        };
                }).ToArray();
            return result;
        }

        public static string GetExecutingAssemblyDirectory()
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }

        public static string FindPathInAllParentDirectories(string path, string relativePath)
        {
            var directories = path.Split(Path.DirectorySeparatorChar);
            for(var i = directories.Length - 1; i > 0; i--)
            {
                var combinedPath = Path.Combine(path, relativePath);
                if(Directory.Exists(combinedPath))
                    return combinedPath;
                var directoryName = directories[i];
                path = path.TrimEnd(Path.DirectorySeparatorChar);
                path = path.Substring(0, path.Length - directoryName.Length);
            }
            return null;
        }

        /// <summary>
        ///     Creates a relative path from one file or folder to another.
        /// </summary>
        /// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
        /// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
        /// <returns>The relative path from the start directory to the end path.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="UriFormatException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static string MakeRelativePath(string fromPath, string toPath)
        {
            if(String.IsNullOrEmpty(fromPath)) 
                throw new ArgumentNullException("fromPath");
            if(String.IsNullOrEmpty(toPath)) 
                throw new ArgumentNullException("toPath");

            var fromUri = new Uri(fromPath);
            var toUri = new Uri(toPath);

            if(fromUri.Scheme != toUri.Scheme)
                return toPath;

            var relativeUri = fromUri.MakeRelativeUri(toUri);
            var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if(toUri.Scheme.ToUpperInvariant() == "FILE")
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

            return relativePath;
        }

        private static void DeleteDirectoryWithRetries(string path)
        {
            for(var attempt = 0; attempt < 10; attempt++)
            {
                try
                {
                    if(Directory.Exists(path))
                        Directory.Delete(path, true);
                    break;
                }
                catch(Exception e)
                {
                    logger.Error(string.Format("Error while deleting directory '{0}', attempt={1}", path, attempt), e);
                    Thread.Sleep(TimeSpan.FromSeconds(attempt + 1));
                }
            }
        }

        private static readonly ILog logger = LogManager.GetLogger(typeof(PathExtensions));
    }
}