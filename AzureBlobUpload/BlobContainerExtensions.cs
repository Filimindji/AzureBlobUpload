using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureBlobUpload
{
    public static class BlobContainerExtensions
    {
        public static void Rename(this CloudBlobContainer container, string oldName, string newName)
        {
            RenameAsync(container, oldName, newName).Wait();
        }

        public static async Task RenameAsync(this CloudBlobContainer container, string oldName, string newName)
        {
            var source = await container.GetBlobReferenceFromServerAsync(oldName);
            var target = container.GetBlockBlobReference(newName);

            await target.StartCopyFromBlobAsync(source.Uri);

            while (target.CopyState.Status == CopyStatus.Pending)
                await Task.Delay(100);

            if (target.CopyState.Status != CopyStatus.Success)
                throw new ApplicationException("Rename failed: " + target.CopyState.Status);

            await source.DeleteAsync();
        }


        public static void Copy(this CloudBlobContainer container, string oldName, string newName)
        {
            CopyAsync(container, oldName, newName).Wait();
        }

        public static async Task CopyAsync(this CloudBlobContainer container, string oldName, string newName)
        {
            var source = await container.GetBlobReferenceFromServerAsync(oldName);
            var target = container.GetBlockBlobReference(newName);

            await target.StartCopyFromBlobAsync(source.Uri);

            while (target.CopyState.Status == CopyStatus.Pending)
                await Task.Delay(100);

            if (target.CopyState.Status != CopyStatus.Success)
                throw new ApplicationException("Copy failed: " + target.CopyState.Status);

        }
    }
}