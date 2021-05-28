using System.Collections.Generic;
using System.Linq;

namespace Stashify.Validation
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<ValidationError> Errors { get; set; }
        public string Message() => string.Join(", ", Errors.Select(x => x.Message));
        
        public ValidationResult()
        {
            IsValid = true;
            Errors = new List<ValidationError>();
        }

    }
}
