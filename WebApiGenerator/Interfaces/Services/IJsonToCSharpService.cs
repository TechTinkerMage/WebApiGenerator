using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApiGenerator.Interfaces.Services
{
    public interface IJsonToCSharpService
    {
        Task<string> GenerateCSharpClassesFromJsonAsync(string json);
    }
}
