using System;

namespace Bhbk.Lib.Aurora.Models
{
    public class Network
    {
        public System.Guid Id { get; set; }
        public Nullable<System.Guid> UserId { get; set; }
        public string UserName { get; set; }
        public int SequenceId { get; set; }
        public string Address { get; set; }
        public int ActionTypeId { get; set; }
        public string Comment { get; set; }
        public System.DateTimeOffset CreatedUtc { get; set; }
        public bool IsEnabled { get; set; }

        public virtual NetworkActionType ActionType { get; set; }
        public virtual Login User { get; set; }
    }
}
