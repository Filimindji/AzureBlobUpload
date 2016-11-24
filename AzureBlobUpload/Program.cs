using System;
using System.IO;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureBlobUpload
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                PrintHelp();
                return;
            }

            switch (args[0])
            {
                case "-u":
                    Upload(args);
                    break;

                case "-d":
                    Download(args);
                    break;

                case "-r":
                    Rename(args);
                    break;

                default:
                    PrintHelp();
                    break;
            }
        }

        private static void Rename(string[] args)
        {
            try
            {
                if (args.Length != 4)
                {
                    PrintHelp();
                    return;
                }

                string container = args[1];
                string filename = args[2];
                string newName = args[3];


                StorageConfig config = new StorageConfig();
                StorageHelper storageHelper = new StorageHelper(config);

                storageHelper.Rename(container, filename, newName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        private static void Download(string[] args)
        {
            try
            {
                if (args.Length != 3)
                {
                    PrintHelp();
                    return;
                }

                string url = args[1];
                string filename = args[2];


                WebRequestHelper webRequestHelper = new WebRequestHelper();
                byte[] bytes = webRequestHelper.GetData(url, null);

                File.WriteAllBytes(filename, bytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }


        private static void Upload(string[] args)
        {
            try
            {
                if (args.Length < 3)
                {
                    PrintHelp();
                    return;
                }


                string container = args[1];
                string path = args[2];

                string subdirectory = null;

                if (args.Length == 4)
                {
                    subdirectory = args[3];
                }

                if (File.Exists(path) == false)
                {
                    Console.WriteLine("No file found");
                    return;
                }

                string mimeType = null;
                string extension = Path.GetExtension(path).ToUpperInvariant();
                switch (extension)
                {
                    case ".EXE":
                        mimeType = "application/x-msdownload";
                        break;

                    case ".DMG":
                        mimeType = "application/octet-stream";
                        break;

                    case ".TXT":
                        mimeType = "text/plain";
                        break;
                }
                
                StorageConfig config = new StorageConfig();
                StorageHelper storageHelper = new StorageHelper(config);

                Uri result = storageHelper.UploadBlob(container, path, mimeType, subdirectory);
                Console.WriteLine("");
                Console.WriteLine("Result : " + result);



            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

        }

        private static void PrintHelp()
        {
            Console.WriteLine("-u <container> <file-to-upload> --OPTIONAL <subfolder>");
            Console.WriteLine("-d <url> <filename>");
            Console.WriteLine("-r <container> <filename> <new-name>");
        }
    }
}
