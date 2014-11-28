using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SysPath = System.IO.Path;

namespace TheAirline.Model.Services.Filesystem
{
    class Path : IFilesystemEntity
    {
        private string path;

        public Path(string path)
        {
            this.path = path;
        }

        public bool Create()
        {
            DirectoryInfo dirInfo = Directory.CreateDirectory(path);
            return dirInfo.FullName.Length > 0;
        }

        public void Delete()
        {
            PathMustExist();
            Directory.Delete(path);
        }

        public List<string> Files()
        {
            PathMustExist();
            return Directory.EnumerateFiles(path).ToList();
        }

        public List<string> Paths()
        {
            PathMustExist();
            return Directory.EnumerateDirectories(path).ToList();
        }

        public bool Exists()
        {
            return Directory.Exists(path);
        }

        public string ToString()
        {
            return path;
        }

        private void PathMustExist()
        {
            if (!Exists())
            {
                throw new EntityDoesntExistException("Path " + path + " does not exist");
            }
        }
    }
}
