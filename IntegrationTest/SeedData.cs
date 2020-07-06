using Domain;
using Services;

namespace IntegrationTest
{
	public static class SeedData
	{
		public static void PopulateTestData(SttContext ctx)
		{

			var student = new User()
			{
				FirstName = "Dante",
				LastName = "Acrich",
				Category = UserCategory.Student
			};
			var parent = new User()
			{
				FirstName = "Javier",
				LastName = "Acrich",
				Category = UserCategory.Parent
			};

			student.Parent = parent;

			var card1 = new Card
			{
				Name = "card1",
				CardCode = "code1",
			};

			var card2 = new Card
			{
				Name = "card2",
				CardCode = "code2",
			};

			var card3 = new Card
			{
				Name = "card3",
				CardCode = "code3",
			};

			var school1 = new School
			{
				Name = "school1",
				Address = "street 123",
				Phone = "123456789",
				Email = "school1@stt.com",
				Id = 1
			};


			var device1 = new Device()
			{
				Name = "device1",
				DeviceCode = "AAAA#1",
				School = school1,
				Type = DeviceType.Outside,
				Id = 1,
			};

			var device2 = new Device()
			{
				Name = "device2",
				DeviceCode = "AAAA#2",
				School = school1,
				Type = DeviceType.Inside,
				Id = 2
			};

			var insideDevice = new Device()
			{
				Name = "device3",
				DeviceCode = "BBBB#3",
				School = school1,
				Type = DeviceType.Inside,
				Id = 3
			};

			var outsideDevice = new Device()
			{
				Name = "device4",
				DeviceCode = "BBBB#4",
				School = school1,
				Type = DeviceType.Outside,
				Id = 4
			};



			var bus1 = new SchoolBus()
			{
				Name = "bus1",
				School = school1,
				Device = device1,
				Id = 1
			};

			//	dbContext.Players.Add(new Player("Wayne", "Gretzky", 183, 84, new DateTime(1961, 1, 26)) { Id = 1, Created = DateTime.UtcNow });
			//dbContext.Players.Add(new Player("Mario", "Lemieux", 193, 91, new DateTime(1965, 11, 5)) { Id = 2, Created = DateTime.UtcNow });

			ctx.Users.Add(parent);
			ctx.Users.Add(student);

			ctx.Cards.Add(card1);
			ctx.Cards.Add(card2);
			ctx.Cards.Add(card3);
			ctx.Schools.Add(school1);

			ctx.Devices.Add(device1);
			ctx.Devices.Add(device2);
			ctx.Devices.Add(insideDevice);
			ctx.Devices.Add(outsideDevice);

			insideDevice.RelatedDevice = outsideDevice;
			ctx.Devices.Update(insideDevice);

			outsideDevice.RelatedDevice = insideDevice;
			ctx.Devices.Update(outsideDevice);


			ctx.SchoolBuses.Add(bus1);


			ctx.SaveChanges();
		}
	}
}
