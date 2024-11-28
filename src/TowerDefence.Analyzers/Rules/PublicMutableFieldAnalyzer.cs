using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TowerDefence.Analyzers.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PublicMutableFieldAnalyzer : DiagnosticAnalyzer
    {
        private const string DiagnosticId = "TD999";
        private const string Title = "Public mutable field detected";
        private const string MessageFormat = "Field '{0}' is public and mutable. Consider making it private, readonly, or using a property.";
        private const string Description = "Public fields should be immutable to prevent unexpected state modifications.";
        private const string Category = "Design";

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
            
            context.RegisterSyntaxNodeAction(AnalyzeField, SyntaxKind.FieldDeclaration);
        }

        private static void AnalyzeField(SyntaxNodeAnalysisContext context)
        {
            var fieldDeclaration = (FieldDeclarationSyntax)context.Node;

            // Skip if the field is not public
            if (!fieldDeclaration.Modifiers.Any(SyntaxKind.PublicKeyword))
                return;

            // Skip if the field is readonly or const
            if (fieldDeclaration.Modifiers.Any(SyntaxKind.ReadOnlyKeyword) ||
                fieldDeclaration.Modifiers.Any(SyntaxKind.ConstKeyword))
                return;

            foreach (var variable in fieldDeclaration.Declaration.Variables)
            {
                var diagnostic = Diagnostic.Create(
                    Rule,
                    variable.GetLocation(),
                    variable.Identifier.Text);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}