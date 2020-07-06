using System;
using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public abstract class BaseEntity : IBaseEntity
    {
        public int Id { get; set; }

        public DateTime? Created { get; set; }

        public Guid? CreatedBy { get; set; }

        public DateTime? Updated { get; set; }

        public Guid? UpdatedBy { get; set; }

        public bool? IsDisabled { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }

    public interface IBaseEntity
    {
        int Id { get; set; }
        DateTime? Created { get; set; }
        Guid? CreatedBy { get; set; }
        DateTime? Updated { get; set; }
        Guid? UpdatedBy { get; set; }
        bool? IsDisabled { get; set; }
    }
}
