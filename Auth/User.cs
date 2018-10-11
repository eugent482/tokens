using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace InternetHospital.WebApi.Auth
{
    [Table("tblUsers")]
    public class User
    {
        [Key]
        public int Id { get; set; }        
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
    }
}
