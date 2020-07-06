using Domain;
using Moq;
using Services;
using System;
using System.Collections.Generic;
using Test;
using Xunit;

namespace IntegrationTest
{
	public class UnitTest1 : BaseTest
	{
		[Theory(Skip = "specific reason")]
		[InlineData("BBBB#3")]
		[InlineData("BBBB#4")]
		public async void ShouldNotSendPushNotification(string deviceCode)
		{
			//arrange
			var dateTimeService = GetDateTimeService();
			var notificationSender = new Mock<INotificationSender>();

			var rfidController = GetRfidController(dateTimeService, notificationSender.Object);

			var model = new PostDeviceEventModel()
			{
				CardCodes = new List<string>() { "code3" },
				DeviceCode = deviceCode
			};

			//act
			var result = await rfidController.Post(model);

			//assert
			var any = It.IsAny<string>();
			notificationSender.Verify(x => x.SendNotificationAsync(any, any, any), Times.Never);
		}



		[Fact(Skip = "specific reason")]
		public async void ShouldSendPushNotificationEnteringSchool()
		{
			//arrange
			var now = DateTime.UtcNow;
			var future = now.AddMinutes(3);
			var notificationSender = new Mock<INotificationSender>();

			var dateTimeService1 = new Mock<IDateTimeService>();
			dateTimeService1.Setup(x => x.UtcNow()).Returns(now);
			var rfidController1 = GetRfidController(dateTimeService1.Object, notificationSender.Object);

			var dateTimeService2 = new Mock<IDateTimeService>();
			dateTimeService1.Setup(x => x.UtcNow()).Returns(future);
			var rfidController2 = GetRfidController(dateTimeService2.Object, notificationSender.Object);

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

			//act
			var result1 = await rfidController1.Post(model1);

			var result2 = await rfidController2.Post(model2);

			//assert
			var any = It.IsAny<string>();
			notificationSender.Verify(x => x.SendNotificationAsync(any, "Dante is entering school", any), Times.Once);



		}

		[Fact(Skip = "specific reason")]
		public async void ShouldSendPushNotificationLeavingSchool()
		{

		}
	}
}
