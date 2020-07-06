using System;

namespace Services
{
	public class DateTimeService : IDateTimeService
	{
		public DateTime UtcNow()
		{
			return DateTime.UtcNow;
		}
	}


	public interface IDateTimeService
	{
		/// <summary>
		/// desgined to mock current time for testing.
		/// </summary>
		DateTime UtcNow();
	}
}
