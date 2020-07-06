
namespace Domain.Models
{
    public class CardModel : IHaveRowVersion
    {
        public string Name { get; set; }

        public string CardCode { get; set; }

        public byte[] RowVersion { get; set; }
    }
}
