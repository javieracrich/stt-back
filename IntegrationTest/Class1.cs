using Api;
using Domain;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace IntegrationTest
{
	public class PlayersControllerIntegrationTests : IClassFixture<SttWebAppFactory<Startup>>
	{
		private readonly HttpClient _client;

		public PlayersControllerIntegrationTests(SttWebAppFactory<Startup> factory)
		{
			_client = factory.CreateClient();
		}

		[Fact(Skip = "adasd")]
		public async Task ShouldNotSendPush()
		{

			var model1 = new PostDeviceEventModel()
			{
				CardCodes = new List<string>() { "code3" },
				DeviceCode = "BBBB#3" ///inside
			};

			var model2 = new PostDeviceEventModel()
			{
				CardCodes = new List<string>() { "code3" },
				DeviceCode = "BBBB#4" ///outside
			};

			// The endpoint or route of the controller action.
			var httpResponse = await _client.PostAsJsonAsync("/rfid", model1);

			// Must be successful.
			httpResponse.EnsureSuccessStatusCode();

			// Deserialize and examine results.
			//	var stringResponse = await httpResponse.Content.ReadAsStringAsync();
			//	var players = JsonConvert.DeserializeObject<IEnumerable<Player>>(stringResponse);
			//	Assert.Contains(players, p => p.FirstName == "Wayne");
			//	Assert.Contains(players, p => p.FirstName == "Mario");
		}
	}
}
