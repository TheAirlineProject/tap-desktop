using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TheAirline.Model.Services
{
    /**
     * Responsible for the creation and deletion of directories
     */
    class Filesystem
    {
        public bool CreatePath(string path)
        {
            var dirInfo = Directory.CreateDirectory(path);
            return dirInfo is DirectoryInfo;
        }

        public bool DeletePath(string path, bool recursive = false)
        {
            Directory.Delete(path, recursive);
            return true;
        }

        public bool PathExists(string path)
        {
            return Directory.Exists(path);
        }

        
        public bool CreateIfNotExists(string path)
        {
            if (!PathExists(path))
            {
                CreatePath(path);
            }

            return true;
        }

        public bool CreateIfNotExists(List<string> paths)
        {
            bool success = true;

            for(int i = 0; i < paths.Count; i++)
            {
                if (! CreateIfNotExists(paths[i]))
                {
                    success = false;
                }
            }

            return success;
        }

        public List<string> ListPathFiles(string path)
        {
            var contents = Directory.EnumerateFiles(path);
            return contents.ToList();
        }

        public Array ListPathFiles(List<string> paths)
        {
            List<string>[] results = new List<string>[paths.Count];

            for (int i = 0; i < paths.Count; i++)
            {
                results[i] = ListPathFiles(paths[i]);
            }

            return results;
        }
    }
}
