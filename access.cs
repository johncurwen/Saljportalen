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
    
    public partial class access
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public access()
        {
            this.employees = new HashSet<employee>();
        }
    
        public string AccessID { get; set; }
        public bool AcCompetitions { get; set; }
        public bool AcEmployees { get; set; }
        public bool AcStats { get; set; }
        public bool AcAccess { get; set; }
        public bool AcUsers { get; set; }
        public bool AcBranches { get; set; }
        public bool AcWeb { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<employee> employees { get; set; }
    }
}