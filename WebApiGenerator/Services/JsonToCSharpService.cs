using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;
using System.Linq;
using System.Threading.Tasks;
using WebApiGenerator.Interfaces;
using WebApiGenerator.Interfaces.Services;

namespace WebApiGenerator.Services
{
    public class JsonToCSharpService : IJsonToCSharpService, ISingletonService
    {
        public async Task<string> GenerateCSharpClassesFromJsonAsync(string json)
        {
            string code = "Failed to generate code";
            try
            {
                var schema = JsonSchema.FromSampleJson(json);
                if (schema == null) return code;

                var settings = new CSharpGeneratorSettings
                {
                    ClassStyle = CSharpClassStyle.Poco,
                };

                var generator = new CSharpGenerator(schema, settings);
                var file = generator.GenerateFile();
                var tree = CSharpSyntaxTree.ParseText(file);
                var root = tree.GetRoot().WithoutTrivia();
                //получить namespace node
                var namespaceNode = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
                var newNamespaceNode = namespaceNode.WithName(SyntaxFactory.IdentifierName("WebApiGenerator.Services"));

                // Создаем rewriter для удаления атрибутов.
                var rewriter = new RemoveAttributesRewriter();
                var newRoot = rewriter.Visit(root);

                code = newRoot.NormalizeWhitespace().ToFullString();

                return code;
            }
            catch (System.Exception)
            {
                return code;
            }
        }
    }
    class RemoveAttributesRewriter : CSharpSyntaxRewriter
    {
        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            // Удаление атрибутов из класса.
            var newNode = node
                .WithAttributeLists(SyntaxFactory.List<AttributeListSyntax>());
            //.WithModifiers(SyntaxFactory.TokenList(node.Modifiers.Where(x => !x.IsKind(SyntaxKind.PartialKeyword))));

            // Удаление атрибутов из свойств.
            newNode = newNode.RemoveNodes(
                newNode.DescendantNodes().OfType<AttributeListSyntax>(), SyntaxRemoveOptions.AddElasticMarker);

            return base.VisitClassDeclaration(newNode);
        }
    }
}