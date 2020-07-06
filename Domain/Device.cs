
using Domain.Models;

namespace Domain
{
    public class Device : BaseEntity, IBaseEntity, IHaveRowVersion
    {
        public DeviceType Type { get; set; }
        public string Name { get; set; }
        public string DeviceCode { get; set; }
        public virtual School School { get; set; }
        public virtual Bus Bus { get; set; }
        public virtual Device RelatedDevice { get; set; }
    }
}
