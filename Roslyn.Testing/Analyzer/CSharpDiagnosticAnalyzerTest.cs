using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslyn.Testing.Model;
using Shouldly;

namespace Roslyn.Testing.Analyzer
{
	/// <summary>
	/// Base class for testing Analyzer
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class CSharpDiagnosticAnalyzerTest<T> : FileReaderTest
		where T : DiagnosticAnalyzer, new()
	{
		/// <inheritdoc />
		public override string Filepath => _diagnosticAnalyzer.GetType().Name;

		/// <inheritdoc />
		public override string PathToTestData => "./TestData/Analyzer/";

		private readonly DiagnosticAnalyzer _diagnosticAnalyzer;

		protected CSharpDiagnosticAnalyzerTest()
		{
			_diagnosticAnalyzer = new T();
		}

		/// <summary>
		/// Get additional references.
		/// </summary>
		/// <remarks>
		/// Can be used when you want testing analyzer for concrete external reference.
		/// By default: empty enumerable
		/// </remarks>
		/// <returns>Enumerable external metadata references</returns>
		[UsedImplicitly]
		protected virtual IEnumerable<MetadataReference> GetAdditionalReferences()
		{
			return Enumerable.Empty<MetadataReference>();
		}

		/// <summary>
		/// Called to test a C# DiagnosticAnalyzer when applied on the single inputted string as a source
		/// </summary>
		/// <remarks>input a DiagnosticResult for each Diagnostic expected</remarks>
		/// <param name="source"> A class in the form of a string to run the analyzer on </param>
		/// <param name="expected">
		/// DiagnosticResults that should appear after the analyzer is run on the source
		/// </param>
		[UsedImplicitly]
		protected void VerifyDiagnostic(string source, DiagnosticResult[] expected)
		{
			VerifyDiagnostic(new[] { source }, expected);
		}

		/// <summary>
		/// Called to test a C# DiagnosticAnalyzer when applied on the single inputted string as a source
		/// </summary>
		/// <remarks>input a DiagnosticResult for each Diagnostic expected</remarks>
		/// <param name="source"> A class in the form of a string to run the analyzer on </param>
		/// <param name="expected">
		/// DiagnosticResult that should appear after the analyzer is run on the source
		/// </param>
		[UsedImplicitly]
		protected void VerifyDiagnostic(string source, DiagnosticResult expected)
		{
			VerifyDiagnostic(new[] { source }, new[] { expected });
		}

		[UsedImplicitly]
		protected void VerifyNoDiagnosticTriggered(string source)
		{
			VerifyDiagnostic(new[] { source },  Array.Empty<DiagnosticResult>());
		}

		/// <summary>
		/// Called to test a C# DiagnosticAnalyzer when applied on the single inputted string as a source
		/// </summary>
		/// <remarks>input a DiagnosticResult for each Diagnostic expected</remarks>
		/// <param name="sources"> A classes in the form of a string to run the analyzer on </param>
		/// <param name="expected">
		/// DiagnosticResults that should appear after the analyzer is run on the source
		/// </param>
		[UsedImplicitly]
		protected void VerifyDiagnostic(string[] sources, DiagnosticResult[] expected)
		{
			var actual = _diagnosticAnalyzer.GetSortedDiagnostics(sources, LanguageNames.CSharp, GetAdditionalReferences());
			var result = _diagnosticAnalyzer.VerifyDiagnosticResults(actual, expected);
			result.Success.ShouldBeTrue(result.ErrorMessage);
		}
	}
}