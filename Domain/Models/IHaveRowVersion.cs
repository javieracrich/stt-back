namespace Domain.Models
{
    public interface IHaveRowVersion
    {
        byte[] RowVersion { get; set; }
    }
}
