using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogPlatformCleanArchitecture.Application.Configurations
{
    public class JWT
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int Lifetime { get; set; }
        public string SigningKey { get; set; }
    }
}
