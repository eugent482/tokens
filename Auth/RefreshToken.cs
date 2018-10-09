using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace InternetHospital.WebApi.Auth
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User ")]
        public int UserId { get; set; }

        public string Token { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime ExpiresDate { get; set; }

        public bool Revoked { get; set; }

        public virtual ApplicationUser User { get; set; }
    }
}
