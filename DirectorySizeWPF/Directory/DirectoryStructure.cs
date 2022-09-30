using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectorySizeWPF
{
    public static class DirectoryStructure
    {
        public static List<DirectoryItem> GetLogicalDrive()
        {
            return Directory.GetLogicalDrives().Select(drive => new DirectoryItem { FullPath = drive, Type = DirectoryItemType.Drive }).ToList();
        }

        public static List<DirectoryItem> GetDirectoryContents(string fullPath)
        {
            var items = new List<DirectoryItem>();
            DirectoryInfo directoryInfo = new DirectoryInfo(fullPath);

            try
            {
                foreach (var directory in directoryInfo.GetDirectories())
                {
                    var isHidden = (directory.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
                    var isSystem = (directory.Attributes & FileAttributes.System) == FileAttributes.System;
                    if (!isHidden && !isSystem)
                    {
                        items.Add(new DirectoryItem { FullPath = directory.FullName, Type = DirectoryItemType.Folder, Size = 0 });
                    }
                }
            }
            catch { }

            try
            {
                foreach (var file in directoryInfo.GetFiles())
                {
                    var isHidden = (file.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
                    var isSystem = (file.Attributes & FileAttributes.System) == FileAttributes.System;
                    if (!isHidden && !isSystem)
                    {
                        items.Add(new DirectoryItem { FullPath = file.FullName, Type = DirectoryItemType.File, Size = 0 });
                    }
                }
            }
            catch
            {

            }

            return items;
        }

        public static string GetFileFolderName(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            var normalizedPath = path.Replace('/', '\\');

            var lastIndex = normalizedPath.LastIndexOf('\\');

            if (lastIndex <= 0)
            {
                return path;
            }
            return path.Substring(lastIndex + 1);
        }
    }
}
