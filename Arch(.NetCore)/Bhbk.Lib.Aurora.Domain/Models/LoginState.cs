using System;
using System.Text.Json.Serialization;

namespace Bhbk.Lib.Aurora.Domain.Models
{
    public class LoginState
    {
        public int SessionId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        [JsonIgnore]
        public string Password { get; set; }
        public bool IsPasswordRequired { get; set; }
        public bool IsPasswordAuthComplete { get; set; }
        public bool IsPublicKeyRequired { get; set; }
        public bool IsPublicKeyAuthComplete { get; set; }
    }
}
