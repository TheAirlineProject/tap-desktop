using System;

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
            : base(message, inner)
        {

        }
    }
}