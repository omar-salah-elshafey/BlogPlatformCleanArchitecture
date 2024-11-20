using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogPlatformCleanArchitecture.Application.DTOs
{
    public class ChangeUserRoleDto
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Role { get; set; }
    }
}
