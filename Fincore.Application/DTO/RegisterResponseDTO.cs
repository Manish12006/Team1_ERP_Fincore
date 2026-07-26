using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fincore.Application.DTO
{
    public class RegisterResponseDTO
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string RoleName { get; set; }
        public string UserCategory { get; set; }
        public bool Is2FAEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
