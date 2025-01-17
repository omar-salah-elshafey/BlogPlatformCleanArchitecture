using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogPlatformCleanArchitecture.Application.ExceptionHandling
{
    public class NullOrWhiteSpaceInputException : Exception
    {
        public NullOrWhiteSpaceInputException(string message) : base(message) { }
    }
}
