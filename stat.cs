//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SakraStats
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    public partial class stat
    {
        public int StatsID { get; set; }
        public string EmployeeID { get; set; }
        public Nullable<int> Contacts { get; set; }
        public Nullable<int> Demos { get; set; }
        public Nullable<int> Sales { get; set; }
        public Nullable<int> Turnover { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> Date { get; set; }
        public Nullable<int> Service { get; set; }
    
        public virtual employee employee { get; set; }
    }
}
