using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApiGenerator.Interfaces.Services;

namespace WebApiGenerator.Services
{
    public class DocumentProcessingService : IDocumentProcessingService
    {
        public async Task<string> ProcessDocument(Document document)
        {
            var model = await document.GetSemanticModelAsync();
            var nodes = (await document.GetSyntaxRootAsync()).DescendantNodes();
            var classesAndRecords = nodes.OfType<TypeDeclarationSyntax>()
                             .Where(n => n is ClassDeclarationSyntax
                                      || n is RecordDeclarationSyntax
                                      || n is InterfaceDeclarationSyntax);

            if (!classesAndRecords.Any()) return string.Empty;

            var stringBuilder = new StringBuilder();
            foreach (var obj in classesAndRecords)
            {
                stringBuilder.AppendLine(obj.NormalizeWhitespace().ToFullString());
            }

            return stringBuilder.ToString();
        }
    }
}
