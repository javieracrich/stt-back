namespace Domain.Models
{
	public class FirebasePushModel
	{
		// List of all devices assigned to a user
		public string[] deviceTokens { get; set; }
		// Title of notification
		public string title { get; set; }
		// Description of notification
		public string body { get; set; }
		// Object with all extra information you want to send hidden in the notification
		public object data { get; set; }

	}


	public class FirebaseMessage
	{
		public string[] registration_ids { get; set; }
		public FirebaseNotification notification { get; set; }
		public object data { get; set; }
	}
	public class FirebaseNotification
	{
		public string title { get; set; }
		public string text { get; set; }
	}

}
