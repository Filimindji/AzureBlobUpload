using System.IO;

namespace AzureBlobUpload
{
    public class StorageConfig
    {
        public StorageConfig()
        {
            ConnexionString = File.ReadAllText("config.txt");
        }



        public string ConnexionString { get; private set; }
    }
}