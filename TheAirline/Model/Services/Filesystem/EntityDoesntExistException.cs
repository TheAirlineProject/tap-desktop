using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheAirline.Model.Services.Filesystem
{
    class EntityDoesntExistException : Exception
    {
        public EntityDoesntExistException()
        {

        }

        public EntityDoesntExistException(string message)
            : base(message)
        {

        }

        public EntityDoesntExistException(string message, Exception inner)
            :base(message, inner)
        {

        }
    }
}
