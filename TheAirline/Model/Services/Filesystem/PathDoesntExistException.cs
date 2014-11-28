using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheAirline.Model.Services.Filesystem
{
    class PathDoesntExistException : Exception
    {
        public PathDoesntExistException()
        {

        }

        public PathDoesntExistException(string message)
            : base(message)
        {

        }

        public PathDoesntExistException(string message, Exception inner)
            :base(message, inner)
        {

        }
    }
}
