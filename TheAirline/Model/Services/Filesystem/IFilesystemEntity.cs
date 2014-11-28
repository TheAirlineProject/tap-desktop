namespace TheAirline.Model.Services.Filesystem
{
    interface IFilesystemEntity
    {
        bool Exists();

        bool Create();

        void Delete();

        void Require();
    }
}
