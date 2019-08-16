using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApiJwt.Models
{
    public class Foundation
    {

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
       

        public Foundation()
        {
            
            this.Projects = new HashSet<Project>();
        }

        [Key]
        public int idFoundation { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public string phoneNumber { get; set; }
        public string person { get; set; }
        public double lat { get; set; }
        public double lng { get; set; }
        public string url { get; set; }

        [JsonIgnore]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Project> Projects { get; set; }


    }
}