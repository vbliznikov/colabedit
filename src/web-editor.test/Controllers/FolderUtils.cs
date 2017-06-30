using System;
using System.IO;
using CollabEdit.IO;
using System.Linq;

namespace CollabEdit.Controllers.Test
{
    public class FolderUtil
    {
        public string RootPath { get; }
        public FolderUtil(string rootPath)
        {
            RootPath = rootPath;
        }

        public void Clear()
        {
            var dirInfo = new DirectoryInfo(RootPath);

            if (!dirInfo.Exists) return;

            foreach (var fsFile in dirInfo.EnumerateFiles())
                fsFile.Delete();
            foreach (var fsDir in dirInfo.EnumerateDirectories())
                fsDir.Delete(true);
        }

        public FolderUtil CreateFolder(string name)
        {
            var folderPath = FilePath.Combine(RootPath, name);
            Directory.CreateDirectory(folderPath);
            return new FolderUtil(folderPath);
        }

        public FolderUtil CreateFolders(short count)
        {
            for (short i = 1; i <= count; i++)
            {
                var folderName = string.Format("folder-{0}", i);
                CreateFolder(folderName);
            }
            return this;
        }

        public int FolderCount
        {
            get
            {
                return Directory.EnumerateDirectories(RootPath).Count();
            }
        }

        public bool FolderExists(string folderName)
        {
            return Directory.Exists(FilePath.Combine(RootPath, folderName));
        }

        public bool FolderIsEmpty(string folderName = "")
        {
            var fullPath = FilePath.Combine(RootPath, folderName);
            var dirInfo = new DirectoryInfo(fullPath);
            if (!dirInfo.Exists)
                throw new InvalidOperationException(string.Format("Folder {0} does not exists.", fullPath));

            if (dirInfo.EnumerateFileSystemInfos().Count() == 0)
                return true;
            else
                return false;
        }

        public FolderUtil CreateFile(string name)
        {
            using (File.Create(FilePath.Combine(RootPath, name))) { }
            return this;
        }

        public FolderUtil CreateFileWithContent(string fileName, string content)
        {
            var fullPath = FilePath.Combine(RootPath, fileName);
            using (Stream stream = File.OpenWrite(fullPath))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(content);
                }
            }
            return this;
        }

        public string ReadFile(string fileName)
        {
            var fullPath = FilePath.Combine(RootPath, fileName);
            using (StreamReader reader = File.OpenText(fullPath))
                return reader.ReadToEnd();
        }

        public FolderUtil CreateFiles(short count)
        {
            for (short i = 1; i <= count; i++)
            {
                var filename = string.Format("file-{0}", i);
                CreateFile(filename);
            }

            return this;
        }

        public int FilesCount
        {
            get
            {
                return Directory.EnumerateFiles(RootPath).Count();
            }
        }

        public bool FileExists(string filename)
        {
            return File.Exists(FilePath.Combine(RootPath, filename));
        }
    }
}