using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
	public class ImageService : IImageService
	{
		private readonly IConfiguration _configuration;
		const double ONE_MB = 1048576;

		public ImageService(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public async Task<FileUploadResult> UploadAsync(string entityId, IFormFile file)
		{
			var validExtensions = new List<string> { "jpg", "gif", "png" };

			var validContentTypes = new List<string> { "image/jpeg", "image/pjpeg", "image/gif", "image/png", "image/x-png" };

			var result = new FileUploadResult();

			//validate content type
			if (validContentTypes.All(e => file.ContentType != e))
			{
				result.Error = "File is not an image. Valid Extensions are jpg, gif and png.";
				result.Success = false;
				return result;
			}
			// validate extensions
			if (!validExtensions.Any(e => file.FileName.EndsWith(e)))
			{
				result.Error = "File extension is invalid. Valid Extensions are jpg, gif and png.";
				result.Success = false;
				return result;
			}
			// validate length;
			if (file.Length > 5 * ONE_MB)
			{
				result.Error = "File should have 5MB max";
				result.Success = false;
				return result;
			}

			var block = await GetCloudBlockBlob(entityId);

			var stream = file.OpenReadStream();

			await block.UploadFromStreamAsync(stream);

			stream.Dispose();

			result.Location = block.Uri?.ToString();

			result.Success = true;

			return result;
		}
		public async Task<bool> DeleteImage(string fileName)
		{
			var block = await GetCloudBlockBlob(fileName);

			return await block.DeleteIfExistsAsync();
		}
		private async Task<CloudBlockBlob> GetCloudBlockBlob(string fileName)
		{
			var cnn = _configuration["Storage:ConnectionString"];

			var storageAccount = CloudStorageAccount.Parse(cnn);

			var blobClient = storageAccount.CreateCloudBlobClient();

			var container = blobClient.GetContainerReference("images");

			await container.CreateIfNotExistsAsync();

			var block = container.GetBlockBlobReference(fileName);

			block.Properties.ContentType = "image/jpg";

			return block;
		}

	}

	public interface IImageService
	{
		Task<FileUploadResult> UploadAsync(string fileName, IFormFile file);
		Task<bool> DeleteImage(string fileName);
	}
}
