using Common;
using System;

namespace Domain.Models
{
	public class LocationModel
	{
		public double Lat { get; set; }

		public double Lng { get; set; }

		public DateTime? DateTime { get; set; }

		public string BusName { get; set; }

        public Guid? BusCode { get; set; }

        public string SupervisorName { get; set; }

		public string Status { get; set; }

        public PushType PushType { get; set; }

        public bool NotFound { get; set; }
	}
}
