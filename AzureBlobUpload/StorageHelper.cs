using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureBlobUpload
{
    public class StorageHelper
    {
        private StorageConfig _configuration;

        public StorageHelper(StorageConfig configuration)
        {
            _configuration = configuration;
        }

        public CloudBlockBlob GetBlockBlobReference(string containerName, string blobName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_configuration.ConnexionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container 
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            // Create the container if it doesn't already exist
            container.CreateIfNotExists();

            // Retrieve reference to a blob named blobName
            return container.GetBlockBlobReference(blobName);
        }


        private const int MaxBlockSize = 4000000; // Approx. 4MB chunk size

        public Uri UploadBlob(string containerName, string filePath, string contentType)
        {
            byte[] fileContent = File.ReadAllBytes(filePath);
            string blobName = Path.GetFileName(filePath);
            return UploadBlob(fileContent, containerName, blobName, contentType);
        }

        private Uri UploadBlob(byte[] fileContent, string containerName, string blobName, string contentType)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_configuration.ConnexionString);
            CloudBlobClient blobclient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobclient.GetContainerReference(containerName);
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
            HashSet<string> blocklist = new HashSet<string>();
            foreach (FileBlock block in GetFileBlocks(fileContent))
            {
                blob.PutBlock(
                    block.Id,
                    new MemoryStream(block.Content, true),
                    null
                    );
                blocklist.Add(block.Id);
            }

            blob.PutBlockList(blocklist);

            if (contentType != null)
            {
                blob.FetchAttributes();
                blob.Properties.ContentType = contentType;
                blob.SetProperties();
            }


            return blob.Uri;
        }
        private IEnumerable<FileBlock> GetFileBlocks(byte[] fileContent)
        {
            HashSet<FileBlock> hashSet = new HashSet<FileBlock>();
            if (fileContent.Length == 0)
                return new HashSet<FileBlock>();
            int blockId = 0;
            int ix = 0;
            int currentBlockSize = MaxBlockSize;
            while (currentBlockSize == MaxBlockSize)
            {
                if ((ix + currentBlockSize) > fileContent.Length)
                    currentBlockSize = fileContent.Length - ix;
                byte[] chunk = new byte[currentBlockSize];
                Array.Copy(fileContent, ix, chunk, 0, currentBlockSize);
                hashSet.Add(
                    new FileBlock()
                    {
                        Content = chunk,
                        Id = Convert.ToBase64String(System.BitConverter.GetBytes(blockId))
                    });
                ix += currentBlockSize;
                blockId++;
            }
            return hashSet;
        }

        internal class FileBlock
        {
            public string Id
            {
                get;
                set;
            }
            public byte[] Content
            {
                get;
                set;
            }
        }

        public MemoryStream LegacyDownloadBlob(string container, string blobName)
        {
            CloudBlockBlob blob = GetBlockBlobReference(container, blobName);
            if (blob == null)
                return null;

            if (blob.Exists() == false)
                return null;

            MemoryStream stream = new MemoryStream();
            blob.DownloadToStream(stream);
            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }

        public bool Exists(string container, string blobName)
        {
            CloudBlockBlob blob = GetBlockBlobReference(container, blobName);
            if (blob == null)
                return false;

            return blob.Exists();
        }


        public void Rename(string containerName, string oldName, string newName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_configuration.ConnexionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container 
            CloudBlobContainer containerReference = blobClient.GetContainerReference(containerName);
            containerReference.Rename(oldName, newName);
        }

        public void Copy(string containerName, string oldName, string newName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_configuration.ConnexionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container 
            CloudBlobContainer containerReference = blobClient.GetContainerReference(containerName);
            containerReference.Copy(oldName, newName);
        }
    }
}