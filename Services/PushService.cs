using Domain.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Services
{

	/// <summary>
	/// Send push notifications using Firebase
	/// </summary>
	public class PushService : IPushService
	{
		private readonly IConfiguration _configuration;

		public PushService(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public async Task<bool> Push(FirebasePushModel model)
		{
			bool sent = false;

			if (!model.deviceTokens.Any())
			{
				return sent;
			}

			//Object creation

			var messageInformation = new FirebaseMessage()
			{
				notification = new FirebaseNotification()
				{
					title = model.title,
					text = model.body
				},
				data = model.data,
				registration_ids = model.deviceTokens
			};

			//Object to JSON STRUCTURE => using Newtonsoft.Json;
			string jsonMessage = JsonConvert.SerializeObject(messageInformation);
			/*
			------ JSON STRUCTURE ------
			{
			notification: {
							title: "",
							text: ""
							},
			data: {
					action: "Play",
					playerId: 5
					},
			registration_ids = ["id1", "id2"]
			}
			------ JSON STRUCTURE ------
			*/

			//Create request to Firebase API
			var url = _configuration["Firebase:PushNotificationUrl"];

			var request = new HttpRequestMessage(HttpMethod.Post, url);

			var serverKey = _configuration["Firebase:ServerKey"];

			request.Headers.TryAddWithoutValidation("Authorization", "key=" + serverKey);
			request.Content = new StringContent(jsonMessage, Encoding.UTF8, "application/json");

			HttpResponseMessage result;
			using (var client = new HttpClient())
			{
				result = await client.SendAsync(request);
				sent = sent && result.IsSuccessStatusCode;
			}
			return sent;
		}
	}

	public interface IPushService
	{
		Task<bool> Push(FirebasePushModel model);
	}
}
