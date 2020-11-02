using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace System.IO.Abstractions.Analyzers.Analyzers.FileSystemTypeAnalyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class StreamReaderAnalyzer : BaseFileSystemNodeAnalyzer
	{
		/// <summary>
		/// Diagnostic Identifier
		/// </summary>
		[UsedImplicitly]
		public const string DiagnosticId = "IO0011";

		/// <summary>
		/// Diagnostic Title
		/// </summary>
		private const string Title = "Replace StreamReader string constructor overload with stream based overload"
			+ " using a stream from IFileSystem.FileStream for improved testability";

		/// <summary>
		/// Diagnostic Message Format
		/// </summary>
		public const string MessageFormat = Title;

		/// <summary>
		/// Diagnostic Description
		/// </summary>
		private const string Description = Title;

		/// <summary>
		/// Правило
		/// </summary>
		private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId,
			Title,
			MessageFormat,
			Category,
			DiagnosticSeverity.Warning,
			true,
			Description);

		/// <inheritdoc />
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

		/// <inheritdoc />
		protected override void Analyze(SyntaxNodeAnalysisContext context, ExpressionSyntax syntax)
		{
			if (IsFirstConstructorParameterOfType<string>(context, syntax))
				context.ReportDiagnostic(Diagnostic.Create(Rule, syntax.GetLocation()));
		}

		/// <inheritdoc />
		protected override Type GetFileSystemType()
		{
			return typeof(StreamReader);
		}
	}
}