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
    
    public partial class Baby
    {
        public long ID { get; set; }
        public System.DateTime Date_Created { get; set; }
        public System.DateTime Date_Modified { get; set; }
        public long UserID { get; set; }
        public string Name { get; set; }
        public System.DateTime Birthday { get; set; }
        public Nullable<double> WeightPounds { get; set; }
        public Nullable<double> WeightOunces { get; set; }
        public Nullable<double> Height { get; set; }
        public string ImageURL { get; set; }
    
        public virtual User User { get; set; }
    }
}
