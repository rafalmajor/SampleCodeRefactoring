using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace SampleCodeRefactoring
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(SampleCodeRefactoringCodeRefactoringProvider)), Shared]
    internal class SampleCodeRefactoringCodeRefactoringProvider : CodeRefactoringProvider
    {
        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var node = root.FindNode(context.Span);

            var typeDecl = node as TypeDeclarationSyntax;
            if (typeDecl == null || context.Document.Name.ToLowerInvariant() == typeDecl.Identifier.ToString().ToLowerInvariant() + ".cs")
            {
                return;
            }

            var action = CodeAction.Create("Move to file", c => Move(context.Document, typeDecl, root));
            context.RegisterRefactoring(action);
        }

        private Task<Solution> Move(Document document, TypeDeclarationSyntax typeDecl, SyntaxNode root)
        {
            var newRoot = root.RemoveNode(typeDecl, SyntaxRemoveOptions.AddElasticMarker);
            var updatedDocument = document.WithSyntaxRoot(newRoot);

            var newDocument = updatedDocument.Project.AddDocument(typeDecl.Identifier + ".cs", SyntaxFactory.CompilationUnit().AddMembers(typeDecl), updatedDocument.Folders);

            return Task.FromResult(newDocument.Project.Solution);
        }
    }
}