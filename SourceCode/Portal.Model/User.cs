//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Portal.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class User
    {
        public User()
        {
            this.Babies = new HashSet<Baby>();
            this.User_Reset_Password = new HashSet<User_Reset_Password>();
            this.TimelineEntries = new HashSet<TimelineEntry>();
        }
    
        public long ID { get; set; }
        public System.DateTime Date_Created { get; set; }
        public System.DateTime Date_Modified { get; set; }
        public string Email { get; set; }
        public string Facebook_ID { get; set; }
        public string EncryptedPassword { get; set; }
        public string Salt { get; set; }
        public string DeviceToken { get; set; }
        public string Token { get; set; }
        public Nullable<System.DateTime> TokenExpireTime { get; set; }
    
        public virtual ICollection<Baby> Babies { get; set; }
        public virtual ICollection<User_Reset_Password> User_Reset_Password { get; set; }
        public virtual ICollection<TimelineEntry> TimelineEntries { get; set; }
    }
}