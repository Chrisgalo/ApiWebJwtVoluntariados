using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApiJwt.Models
{
    public class UsuarioLogin
    {
        [Key]
        public string Usuario { get; set; }
        public string Password { get; set; }
    }
}