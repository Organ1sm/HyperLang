using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Hyper.Generators;

[Generator]
public class NodeGetChildrenGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context) { }

    public void Execute(GeneratorExecutionContext context)
    {
        SourceText text;

        var compilation       = (CSharpCompilation) context.Compilation;
        var imutableArrayType = compilation.GetTypeByMetadataName("System.Collections.Immutable.Immutable.Array`1");
        var separatedSyntaxListType =
            compilation.GetTypeByMetadataName("Hyper.Core.Syntax.SeparatedSyntaxList`1");
        var syntaxNodeType = compilation.GetTypeByMetadataName("Hyper.Core.Syntax.Expr`1");

        var types           = GetAllTypes(compilation.Assembly);
        var syntaxNodeTypes = types.Where(t => !t.IsAbstract && IsPartial(t) && IsDerivedFrom(t, syntaxNodeType));
    }

    private IReadOnlyList<INamedTypeSymbol> GetAllTypes(IAssemblySymbol symbol)
    {
        var result = new List<INamedTypeSymbol>();
        GetAllTypes(result, symbol.GlobalNamespace);
        result.Sort((x, y) => x.MetadataName.CompareTo(y.MetadataName));
        return result;
    }

    private void GetAllTypes(List<INamedTypeSymbol> result, INamespaceOrTypeSymbol symbol)
    {
        if (symbol is INamedTypeSymbol type)
            result.Add(type);

        foreach (var child in symbol.GetMembers())
            if (child is INamespaceOrTypeSymbol nsChild)
                GetAllTypes(result, nsChild);
    }

    private bool IsDerivedFrom(ITypeSymbol? type, INamedTypeSymbol baseType)
    {
        while (type != null)
        {
            if (SymbolEqualityComparer.Default.Equals(type, baseType))
                return true;

            type = type.BaseType;
        }

        return false;
    }

    private bool IsPartial(INamedTypeSymbol type)
    {
        foreach (var declaration in type.DeclaringSyntaxReferences)
        {
            var syntax = declaration.GetSyntax();
            if (syntax is TypeDeclarationSyntax typeDeclaration)
            {
                foreach (var modifer in typeDeclaration.Modifiers)
                {
                    if (modifer.ValueText == "partial")
                        return true;
                }
            }
        }

        return false;
    }
}
