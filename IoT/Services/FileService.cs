using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace IoT.Services
{
    public class FileService
    {
        #region Find and Create Raw Files
        public static async Task<StorageFolder> FindRemovableStorage()
        {
            try
            {
                StorageFolder rootFolder = KnownFolders.RemovableDevices;

                //avoid types like 'Mobile Phone'
                foreach (var folder in await rootFolder.GetFoldersAsync())
                {
                    if (folder.DisplayType != "Mobile Phone" && folder.Attributes == FileAttributes.Directory)
                    {
                        return folder;
                    }
                }
                return null;
            }
            catch
            {
                throw new Exception("FindRemovableStorage exception while finding folder");
            }
        }

        public static async Task<List<StorageFile>> FindFiles()
        {
            try
            {
                var storage = await FindRemovableStorage();

                if (storage != null)
                {
                    var files = await storage.GetFilesAsync();
                    if (files.Any())
                        return files.ToList();
                }
                return null;
            }
            catch
            {
                throw new Exception("FindFiles exception while finding files");
            }
        }

        /// <summary>
        /// Finds a file in the first removable storage, name is the name of the file including the file name extension.
        /// </summary>
        /// <param name="name">
        /// the name of the file including the file name extension.
        /// </param>
        /// <returns></returns>
        public static async Task<StorageFile> FindFile(string name, bool create = false)
        {
            try
            {
                var storage = await FindRemovableStorage();

                if (storage != null)
                {
                    foreach (var file in await storage.GetFilesAsync())
                    {
                        if (file.Name == name)
                        {
                            return file;
                        }
                    }
                }

                if (create)
                {
                    return await CreateFile(name);
                }

                return null;
            }
            catch
            {
                throw new Exception("FindFile exception while finding file");
            }
        }

        public static async Task<StorageFile> CreateFile(string name)
        {
            try
            {
                var storage = await FindRemovableStorage();

                if (storage != null)
                {
                    var newstoragefile = await storage.CreateFileAsync(name, CreationCollisionOption.ReplaceExisting);
                    return newstoragefile;
                }
                return null;
            }
            catch
            {
                throw new Exception("CreateFile exception while creating file");
            }
        }
        #endregion

        #region Read Files
        public static async Task<string> ReadFile(StorageFile file)
        {
            try
            {
                return await FileIO.ReadTextAsync(file);
            }
            catch
            {
                throw new Exception("ReadFile exception while reading file");
            }
        }
        #endregion

        #region Write Files
        public static async Task<bool> WriteFile(StorageFile file, String content)
        {
            try
            {
                await FileIO.WriteTextAsync(file, content);
                return true;
            }
            catch
            {
                throw new Exception("WriteFile exception while writing file");
            }
        }

        public static async Task<bool> AddContentToFile(StorageFile file, String content, string separator = "\r\n")
        {
            try
            {
                await FileIO.AppendTextAsync(file, $"{content}{separator}");
                return true;
            }
            catch
            {
                throw new Exception("AddContentToFile exception while writing file");
            }
        }
        #endregion

        

        #region DateTime Stamp
        public static String GetDateTime()
        {
            return DateTime.Now.ToString("ddMMyyyy_hhmmss");
        }

        public static String GetDate()
        {
            return DateTime.Now.ToString("ddMMyyyy");
        }
        #endregion
    }
}
