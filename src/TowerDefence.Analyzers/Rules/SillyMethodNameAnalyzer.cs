using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TowerDefence.Analyzers.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SillyMethodNameAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "TD002";
        private const string Title = "Method name contains 'Silly'";
        private const string MessageFormat = "Method name '{0}' contains the word 'Silly'. Are you sure about this?";
        private const string Description = "Method names should be professional and descriptive.";
        private const string Category = "Naming";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            
            context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
        }

        private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;
            var methodName = methodDeclaration.Identifier.Text;

            // Changed to use ToUpperInvariant() for case-insensitive comparison
            if (methodName.ToUpperInvariant().Contains("SILLY"))
            {
                var diagnostic = Diagnostic.Create(
                    Rule,
                    methodDeclaration.Identifier.GetLocation(),
                    methodName);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
