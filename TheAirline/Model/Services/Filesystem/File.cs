using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public bool Create(string contents = null)
        {
            MustExist();
            FileStream stream = SysFile.Create(path);
            stream.Write(GetBytes(contents), 0, 0);
            stream.Close();
            return true;
        }

        public void Delete()
        {
            MustExist();
            SysFile.Delete(path);
        }

        private byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        private void MustExist()
        {
            if (!Exists())
            {
                throw new EntityDoesntExistException("File " + path + " does not exist");
            }
        }
    }
}
