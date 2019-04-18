using System.IO.Abstractions.Analyzers.RoslynToken;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace System.IO.Abstractions.Analyzers.CodeActions
{
	/// <inheritdoc />
	public class FileServiceInterfaceInjectionCodeAction : CodeAction
	{
		private readonly ClassDeclarationSyntax _class;

		private readonly Document _document;

		public FileServiceInterfaceInjectionCodeAction(string title, Document document, ClassDeclarationSyntax @class)
		{
			_class = @class;
			_document = document;
			Title = title;
		}

		public override string Title { get; }

		public override string EquivalenceKey => Title;

		protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
		{
			var editor = await DocumentEditor.CreateAsync(_document, cancellationToken).ConfigureAwait(false);

			if (!RoslynClassFileSystem.HasFileSystemField(_class))
			{
				editor.InsertMembers(_class,
					0,
					new SyntaxNode[]
					{
						RoslynClassFileSystem.CreateFileSystemFieldDeclaration()
					});
			}

			ConstructorAddParameter(_class, editor);

			var compilationUnitSyntax = RoslynClassFileSystem.GetCompilationUnit(_class);

			if (!compilationUnitSyntax.Usings.Any())
			{
				return editor.GetChangedDocument();
			}

			var fileSystem = RoslynClassFileSystem.GetUsing(compilationUnitSyntax, Constants.FileSystemNameSpace);

			if (fileSystem != default(UsingDirectiveSyntax))
			{
				return editor.GetChangedDocument();
			}

			var systemIo = RoslynClassFileSystem.GetSystemIoUsing(compilationUnitSyntax);

			if (systemIo == default(UsingDirectiveSyntax))
			{
				editor.InsertBefore(compilationUnitSyntax.Usings.FirstOrDefault(),
					RoslynClassFileSystem.GetFileSystemUsing());
			} else
			{
				editor.InsertAfter(systemIo, RoslynClassFileSystem.GetFileSystemUsing());
			}

			return editor.GetChangedDocument();
		}

		private static ExpressionStatementSyntax CreateAssignmentExpression()
		{
			return SF.ExpressionStatement(SF.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
				SF.IdentifierName(Constants.FieldFileSystemName),
				SF.IdentifierName(Constants.ParameterFileSystemName)));
		}

		private static void ConstructorAddParameter(ClassDeclarationSyntax classDeclaration, SyntaxEditor editor)
		{
			var constructor = RoslynClassFileSystem.HasConstructor(classDeclaration)
				? RoslynClassFileSystem.GetConstructor(classDeclaration)
				: SF.ConstructorDeclaration(classDeclaration.Identifier)
					.WithModifiers(SyntaxTokenList.Create(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));

			var newConstructor = constructor.WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation)
				.NormalizeWhitespace();

			if (!RoslynClassFileSystem.ConstructorHasAssignmentExpression(newConstructor))
			{
				newConstructor = newConstructor.AddBodyStatements(CreateAssignmentExpression());
			}

			if (!RoslynClassFileSystem.ConstructorHasFileSystemParameter(newConstructor))
			{
				var parameter = RoslynClassFileSystem.CreateFileSystemParameterDeclaration();
				newConstructor = newConstructor.AddParameterListParameters(parameter);
			}

			if (RoslynClassFileSystem.HasConstructor(classDeclaration))
			{
				editor.ReplaceNode(constructor, newConstructor);
			} else
			{
				editor.InsertBefore(RoslynClassFileSystem.GetMethod(classDeclaration), newConstructor);
			}
		}
	}
}