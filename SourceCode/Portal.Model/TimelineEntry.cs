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
    
    public partial class TimelineEntry
    {
        public long ID { get; set; }
        public System.DateTime Date_Created { get; set; }
        public long UserID { get; set; }
        public int TypeID { get; set; }
        public string Message { get; set; }
        public Nullable<long> MilestoneID { get; set; }
        public Nullable<long> VideoID { get; set; }
        public int CompletedStatus { get; set; }
    
        public virtual TypeMaster TypeMaster { get; set; }
        public virtual User User { get; set; }
    }
}
