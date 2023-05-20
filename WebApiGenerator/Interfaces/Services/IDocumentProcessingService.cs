using Microsoft.CodeAnalysis;
using System.Threading.Tasks;

namespace WebApiGenerator.Interfaces.Services
{
    public interface IDocumentProcessingService
    {
        Task<string> ProcessDocument(Document document);
    }
}
