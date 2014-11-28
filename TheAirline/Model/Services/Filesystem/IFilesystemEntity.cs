using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheAirline.Model.Services.Filesystem
{
    interface IFilesystemEntity
    {
        public bool Exists();

        public bool Create();

        public void Delete();
    }
}
