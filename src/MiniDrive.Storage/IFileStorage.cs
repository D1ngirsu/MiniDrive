using System;
using System.Collections.Generic;
using System.Text;

namespace MiniDrive.Storage
{
    public interface IFileStorage
    {
        Task<string> SaveAsync(Stream file, string fileName);
        Task<Stream> GetAsync(string path);
        Task DeleteAsync(string path);
        string GetFullPath(string relativePath);
    }

}
