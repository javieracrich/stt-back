
using Domain.Models;

namespace Domain
{
	public class Card : BaseEntity, IBaseEntity, IHaveRowVersion
    {
		public string Name { get; set; }

		public string CardCode { get; set; }

        public virtual School School { get; set; }

        public virtual User User { get; set; }
	}
}
