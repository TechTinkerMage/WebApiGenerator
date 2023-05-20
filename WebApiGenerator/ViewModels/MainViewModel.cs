using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WebApiGenerator.Interfaces;
using WebApiGenerator.Interfaces.Services;

namespace WebApiGenerator.ViewModels
{
    public class MainViewModel : BindableBase, ISingletonService
    {
        public MainViewModel(IJsonToCSharpService jsonToCSharpService,
            IDocumentProcessingService documentProcessingService)
        {
            _jsonToCSharpService = jsonToCSharpService;
            _documentProcessingService = documentProcessingService;
           
            Init();

            OpenClass = new DelegateCommand<object>(async (document) =>
            {
                if (document is not Document doc) return;

                Content = await documentProcessingService.ProcessDocument(doc);
            });
        }

        public string Content { get; set; }

        async void Init()
        {
            using var workspace = MSBuildWorkspace.Create();
            var solution = await workspace.OpenSolutionAsync(@"C:\Terralink\platform-universalstorage-adapter-rtdb\Platform.UniversalStorage.Adapter.Rtdb.sln");
            Projects = new ObservableCollection<Project>(solution.Projects);
        }

        public ICommand OpenClass { get; set; }

        public ObservableCollection<Project> Projects { get; set; } = new ObservableCollection<Project>();

        public string TextJson
        {
            get => _textJson;
            set
            {
                SetProperty(ref _textJson, value);
                Task.Run(async () =>
                {
                    Content = await _jsonToCSharpService.GenerateCSharpClassesFromJsonAsync(value);
                    Generate("WebApi");
                });
            }
        }

        private string _textJson;
        private string _namespace;
        private readonly IJsonToCSharpService _jsonToCSharpService;
        private readonly IDocumentProcessingService _documentProcessingService;

        public void Generate(string @name)
        {
            var unit = SyntaxFactory.CompilationUnit();

            // Добавьте пространство имен.
            var @namespace = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(@name)).NormalizeWhitespace();

            // Создайте класс.
            var classDeclaration = SyntaxFactory.ClassDeclaration("MyController");

            // Добавьте модификаторы доступа public.
            classDeclaration = classDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            var method = @"public Task<IActionResult> GetObjects(int id, string name)
{
    Console.WriteLine(""Hello, world!"");
}";

            var methodDeclaration = SyntaxFactory.ParseMemberDeclaration(method) as MethodDeclarationSyntax;

            // Создание атрибута [ProducesResponseType]
            // со значением (StatusCodes.Status200OK, Type = typeof(IReadOnlyCollection<EnumTypeDto>)
            var attribute = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("ProducesResponseType"));
            var attributeArgument = SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression("StatusCodes.Status200OK"));
            var attributeArgument2 = SyntaxFactory.AttributeArgument(SyntaxFactory.NameEquals(SyntaxFactory.IdentifierName("Type")), null, SyntaxFactory.TypeOfExpression(SyntaxFactory.ParseTypeName("IReadOnlyCollection<EnumTypeDto>")));
            attribute = attribute.AddArgumentListArguments(attributeArgument, attributeArgument2);

            methodDeclaration = methodDeclaration.AddAttributeLists(
          SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attribute)),
          SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("HttpGet")))),
          SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Route"), SyntaxFactory.AttributeArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("myRoute"))))))))
      );

            // Добавьте метод в класс.
            classDeclaration = classDeclaration.AddMembers(methodDeclaration);

            // Добавьте класс в пространство имен.
            @namespace = @namespace.AddMembers(classDeclaration);

            // Добавьте пространство имен в единицу компиляции.
            unit = unit.AddMembers(@namespace);

            // Сформатируйте код и выведите его.
            var code = unit
                .NormalizeWhitespace()
                .ToFullString();

            Result = code;
        }

        public string Result { get; set; }
        public string Namespace
        {
            get => _namespace;
            set
            {
                SetProperty(ref _namespace, value);
                Task.Run(async () =>
                {
                    var tree = CSharpSyntaxTree.ParseText(Content);
                    var root = tree.GetRoot();
                    var namespaceNode = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
                    root = root.ReplaceNode(namespaceNode, namespaceNode.WithName(SyntaxFactory.IdentifierName(value)));
                    Content = root.NormalizeWhitespace().ToFullString();
                    Generate(value);
                });
            }
        }
    }
}
