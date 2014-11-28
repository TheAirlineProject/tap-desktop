using System.IO;
using SysFile = System.IO.File;

namespace TheAirline.Model.Services.Filesystem
{
    class File : IFilesystemEntity
    {
        private string path;

        public File(string path)
        {
            this.path = path;
        }

        public bool Exists()
        {
            return SysFile.Exists(path);
        }

        public bool Create()
        {
            Require();
            FileStream stream = SysFile.Create(path);
            stream.Close();
            return true;
        }

        public void Delete()
        {
            Require();
            SysFile.Delete(path);
        }

        public void Require()
        {
            if (!Exists())
            {
                throw new EntityDoesntExistException("File " + path + " does not exist");
            }
        }
    }
}
