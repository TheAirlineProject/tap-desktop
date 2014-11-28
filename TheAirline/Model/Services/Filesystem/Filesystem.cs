using System;

namespace TheAirline.Model.Services.Filesystem
{
    /**
     * Responsible for the creation and deletion of directories
     */
    class Filesystem
    {
        public File GetOrCreateFile(string path)
        {
            File file = new File(path);
            if (!file.Exists())
            {
                file.Create();
            }

            return file;
        }

        public Path GetOrCreateDirectory(string path)
        {
            Path newPath = new Path(path);
            if (!newPath.Exists())
            {
                newPath.Create();
            }

            return newPath;
        }

        public File GetFile(string path)
        {
            File file = new File(path);
            if (!file.Exists())
            {
                throw new EntityDoesntExistException("File " + path + " does not exist");
            }

            return file;
        }

        public Path GetDirectory(string path)
        {
            Path newPath = new Path(path);
            if (!newPath.Exists())
            {
                throw new EntityDoesntExistException("Directory " + path + " does not exist");
            }

            return newPath;
        }

        public string GetMyDocumentsPath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        public string GetMyDocumentsSubpath(string path)
        {
            return GetMyDocumentsPath() + path;
        }
    }
}
