using System;

namespace Domain.Models
{
    public class AlertModel
    {
        public UserCategory[] UserCategories { get; set; }
        public AlertType AlertType { get; set; }
        public string Message { get; set; }
        public DateTime DateTime { get; set; }
    }
}