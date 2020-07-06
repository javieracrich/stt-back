namespace Domain.Models
{
	public class FileUploadResult
	{
		public bool Success { get; set; }
		public string Location { get; set; }
		public string Error { get; set; }
	}
}
