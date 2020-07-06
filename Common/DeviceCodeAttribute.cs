using System;
using System.ComponentModel.DataAnnotations;

namespace Common
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class DeviceCodeAttribute : RegularExpressionAttribute
	{
		private const string pattern = @"^[a-z|A-Z|0-9]{4}#[0-9]{1}$";

		public DeviceCodeAttribute() : base(pattern)
		{
			ErrorMessage = "The device code format is invalid. The correct format is XXXX#Y where X is a letter or number and Y is a number";
		}
	}
}
