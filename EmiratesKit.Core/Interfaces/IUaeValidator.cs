using EmiratesKit.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmiratesKit.Core.Interfaces
{
    public interface IUaeValidator<TResult> where TResult : ValidationResult
    {
        bool IsValid(string? input);
        TResult Validate(string? input);
    }
}
