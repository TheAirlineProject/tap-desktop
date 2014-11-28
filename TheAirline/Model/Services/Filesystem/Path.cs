using System.Collections.Generic;
using System.Linq;
using System.IO;
using SysPath = System.IO.Path;

namespace TheAirline.Model.Services.Filesystem
{
    class Path : IFilesystemEntity
    {
        private readonly string _path;

        public Path(string path)
        {
            this._path = path;
        }

        public bool Create()
        {
            var dirInfo = Directory.CreateDirectory(_path);
            return dirInfo.FullName.Length > 0;
        }

        public void Delete()
        {
            PathMustExist();
            Directory.Delete(_path);
        }

        public List<string> Files()
        {
            PathMustExist();
            return Directory.EnumerateFiles(_path).ToList();
        }

        public List<string> Paths()
        {
            PathMustExist();
            return Directory.EnumerateDirectories(_path).ToList();
        }

        public bool Exists()
        {
            return Directory.Exists(_path);
        }

        public void Require()
        {
            if (!Exists())
            {
                throw new EntityDoesntExistException("Filesystem object at " + _path + " does not exist");
            }
        }

        public new string ToString()
        {
            return _path;
        }

        private void PathMustExist()
        {
            if (!Exists())
            {
                throw new EntityDoesntExistException("Path " + _path + " does not exist");
            }
        }
    }
}
