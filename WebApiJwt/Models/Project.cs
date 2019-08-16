using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApiJwt.Models
{
    public class Project
    {
        [Key]
        public int idProject { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public int quotas { get; set; }


        [ForeignKey("Foundation")]
        public int idFoundationP { get; set; }

        [JsonIgnore]
        public virtual Foundation Foundation { get; set; }

    }
}