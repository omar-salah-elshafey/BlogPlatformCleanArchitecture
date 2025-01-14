using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogPlatformCleanArchitecture.Application.ExceptionHandling
{
    public class InvalidEmailOrTokenException : Exception
    {
        public InvalidEmailOrTokenException(string message) : base(message) { }
    }
}
