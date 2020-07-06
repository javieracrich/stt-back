
namespace Domain.Models
{
    public class DeviceModel : IHaveRowVersion
    {
        public string Name { get; set; }

        public DeviceType Type { get; set; }

        public string DeviceCode { get; set; }

        public BasicSchoolModel School { get; set; }

        public BasicBusModel Bus { get; set; }

        public BasicDeviceModel RelatedDevice { get; set; }

        public byte[] RowVersion { get; set; }
    }
}
