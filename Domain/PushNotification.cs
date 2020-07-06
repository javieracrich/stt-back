using System;

namespace Domain
{
    public class PushNotification
    {
        public PushType PushType { get; set; }
        public Guid ReceiverCode { get; set; }
        public Guid SchoolCode { get; set; }
        public User Student { get; set; }
        public string Message { get; set; }
        public string CardCode { get; set; }
        public Device Device { get; set; }
    }
}
