using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

using NQuery.Language.Semantic;

namespace NQuery.Language.VSEditor
{
    internal sealed class NQueryCompletionSet : CompletionSet
    {
        private readonly ICompletionSession _session;
        private readonly INQuerySemanticModelManager _semanticModelManager;
        private readonly INQueryGlyphService _glyphService;

        public NQueryCompletionSet(ICompletionSession session, INQuerySemanticModelManager semanticModelManager, INQueryGlyphService glyphService)
        {
            _session = session;
            _semanticModelManager = semanticModelManager;
            _glyphService = glyphService;
            _semanticModelManager.SemanticModelChanged += SemanticModelManagerOnSemanticModelChanged;
            _session.Dismissed += SessionOnDismissed;
            Recalculate();
        }

        private void SessionOnDismissed(object sender, EventArgs e)
        {
            _session.Dismissed -= SessionOnDismissed;
            _semanticModelManager.SemanticModelChanged -= SemanticModelManagerOnSemanticModelChanged;
        }

        private void SemanticModelManagerOnSemanticModelChanged(object sender, EventArgs eventArgs)
        {
            Recalculate();
            SelectBestMatch();
        }

        public override void Recalculate()
        {
            WritableCompletions.Clear();
            var textBuffer = _session.TextView.TextBuffer;
            var snapshot = textBuffer.CurrentSnapshot;
            var position = _session.GetTriggerPoint(textBuffer).GetPosition(snapshot);

            var semanticModel = _semanticModelManager.SemanticModel;
            if (semanticModel == null)
                return;

            var root = semanticModel.Compilation.SyntaxTree.Root;
            var tokenAtPosition = GetIdentifierOrKeywordAtPosition(root, position) ??
                                  GetIdentifierOrKeywordAtPosition(root, position - 1);
            var span = tokenAtPosition == null
                           ? new TextSpan(position, 0)
                           : tokenAtPosition.Value.Span;

            var completions = GetCompletions(semanticModel, position);
            ApplicableTo = snapshot.CreateTrackingSpan(span.Start, span.Length, SpanTrackingMode.EdgeInclusive);
            WritableCompletions.AddRange(completions);

            if (WritableCompletions.Count == 0)
                _session.Dismiss();
        }

        private static SyntaxToken? GetIdentifierOrKeywordAtPosition(SyntaxNode root, int position)
        {
            if (!root.Span.Contains(position))
                return null;

            foreach (var nodeOrToken in root.GetChildren())
            {
                if (nodeOrToken.IsToken)
                {
                    if (nodeOrToken.Span.Contains(position))
                        return nodeOrToken.Kind.IsIdentifierOrKeyword()
                                   ? nodeOrToken.AsToken()
                                   : (SyntaxToken?)null;
                }
                else
                {
                    var result = GetIdentifierOrKeywordAtPosition(nodeOrToken.AsNode(), position);
                    if (result != null)
                        return result;
                }
            }

            return null;
        }

        private IEnumerable<Completion> GetCompletions(SemanticModel semanticModel, int position)
        {
            var root = semanticModel.Compilation.SyntaxTree.Root;

            // We don't want to show a completion when typing an alias name
            var alias = GetAlias(root, position);
            if (alias != null)
                return Enumerable.Empty<Completion>();

            // TODO: Return empty set if we are in the column part of a CTE, such as
            //       WITH x (|

            if (InComment(root, position) || InLiteral(root, position))
                return Enumerable.Empty<Completion>();

            var propertyAccessExpression = GetPropertyAccessExpression(root, position);
            if (propertyAccessExpression != null)
                return GetMemberSymbolCompletions(semanticModel, propertyAccessExpression);

            var symbolCompletions = GetGlobalSymbolCompletions(semanticModel, position);
            var keywordCompletions = GetKeywordCompletions();
            var completions = symbolCompletions.Concat(keywordCompletions).ToArray();
            Array.Sort(completions, (x, y) => x.InsertionText.CompareTo(y.InsertionText));
            return completions;
        }

        private IEnumerable<Completion> GetMemberSymbolCompletions(SemanticModel semanticModel, PropertyAccessExpressionSyntax propertyAccessExpression)
        {
            var symbol = semanticModel.GetSymbol(propertyAccessExpression.Target) as TableInstanceSymbol;
            if (symbol == null)
                return Enumerable.Empty<Completion>();

            return CreateSymbolCompletions(symbol.Table.Columns);
        }

        private static AliasSyntax GetAlias(SyntaxNode root, int position)
        {
            var contains = root.Span.Start <= position && position <= root.Span.End;
            if (!contains)
                return null;

            var nodes = from n in root.GetChildren()
                        where n.IsNode
                        select n.AsNode();

            foreach (var node in nodes)
            {
                var r = node as AliasSyntax;
                if (r != null)
                {
                    if (r.Identifier.Span.Start <= position && position <= r.Identifier.Span.End)
                        return r;
                }

                r = GetAlias(node, position);
                if (r != null)
                    return r;
            }

            return null;
        }

        private static PropertyAccessExpressionSyntax GetPropertyAccessExpression(SyntaxNode root, int position)
        {
            var contains = root.Span.Start <= position && position <= root.Span.End;
            if (!contains)
                return null;

            var nodes = from n in root.GetChildren()
                        where n.IsNode
                        select n.AsNode();

            foreach (var node in nodes)
            {
                var r = node as PropertyAccessExpressionSyntax;
                if (r != null)
                {
                    if (r.Target.Span.Contains(position))
                        return GetPropertyAccessExpression(r.Target, position);

                    if (r.Dot.Span.End <= position && position <= r.Name.Span.End)
                        return r;
                }

                r = GetPropertyAccessExpression(node, position);
                if (r != null)
                    return r;
            }

            return null;
        }

        private bool InComment(SyntaxNode root, int position)
        {
            var contains = root.FullSpan.Start <= position && position <= root.FullSpan.End;
            if (!contains)
                return false;

            var relevantChildren = from n in root.GetChildren()
                                   where n.FullSpan.Start <= position && position <= n.FullSpan.End
                                   select n;

            foreach (var child in relevantChildren)
            {
                if (child.IsNode)
                {
                    if (InComment(child.AsNode(), position))
                        return true;
                }
                else
                {
                    var token = child.AsToken();
                    var inComment = (from t in token.LeadingTrivia.Concat(token.TrailingTrivia)
                                     where InSingleLineComment(t, position) ||
                                           InMultiLineComment(t, position)
                                     select t).Any();
                    if (inComment)
                        return true;
                }
            }

            return false;
        }

        private static bool InSingleLineComment(SyntaxTrivia token, int position)
        {
            return token.Kind == SyntaxKind.SingleLineCommentTrivia &&
                   token.Span.Start <= position &&
                   position <= token.Span.End;
        }

        private static bool InMultiLineComment(SyntaxTrivia token, int position)
        {
            // TODO: This is only true if the comment is terminated.
            //
            // If a comment is unterminated we should consider the end position
            // including.
            //
            // For the time being we consider all comments unterminated.
            return token.Kind == SyntaxKind.MultiLineCommentTrivia &&
                   token.Span.Start <= position &&
                   position <= token.Span.End;
        }

        private bool InLiteral(SyntaxNode root, int position)
        {
            // TODO: This is only true if the literal is terminated (string, date literal).
            //
            // If a literal is unterminated we should consider the end position
            // including.
            //
            // For the time being we consider all literals unterminated.

            var contains = root.FullSpan.Start <= position && position <= root.FullSpan.End;
            if (!contains)
                return false;

            var relevantChildren = from n in root.GetChildren()
                                   where n.FullSpan.Start <= position && position <= n.FullSpan.End
                                   select n;

            foreach (var child in relevantChildren)
            {
                if (child.IsNode)
                {
                    if (InLiteral(child.AsNode(), position))
                        return true;
                }
                else
                {
                    var token = child.AsToken();
                    if (token.Kind.IsLiteral())
                        return true;
                }
            }

            return false;
        }

        private IEnumerable<Completion> GetGlobalSymbolCompletions(SemanticModel semanticModel, int position)
        {
            var symbols = semanticModel.LookupSymbols(position);
            if (!symbols.Any())
                symbols = semanticModel.LookupSymbols(position - 1);

            return from s in symbols
                   group s by s.Name
                   into g
                   select CreateSymbolCompletion(g.Key, g);
        }

        private IEnumerable<Completion> CreateSymbolCompletions(IEnumerable<Symbol> symbols)
        {
            return from s in symbols
                   group s by s.Name
                   into g
                   select CreateSymbolCompletion(g.Key, g);
        }

        private Completion CreateSymbolCompletion(string name, IEnumerable<Symbol> symbols)
        {
            var multiple = symbols.Skip(1).Any();
            if (!multiple)
                return CreateSymbolCompletion(symbols.First());

            var displayText = name;
            var insertionText = name;

            var sb = new StringBuilder();
            sb.Append("Ambiguous Name:");
            foreach (var symbol in symbols)
            {
                sb.AppendLine();
                sb.Append("  ");
                sb.Append(symbol);
            }

            var description = sb.ToString();
            var image = _glyphService.GetGlyph(NQueryGlyph.AmbiguousName);
            return new Completion(displayText, insertionText, description, image, null);
        }

        private Completion CreateSymbolCompletion(Symbol symbol)
        {
            var displayText = symbol.Name;
            var insertionText = symbol.Name;
            var description = symbol.ToString();
            var image = GetImage(symbol);
            return new Completion(displayText, insertionText, description, image, null);
        }

        private IEnumerable<Completion> GetKeywordCompletions()
        {
            var imageSource = _glyphService.GetGlyph(NQueryGlyph.Keyword);
            return from k in SyntaxFacts.GetKeywordKinds()
                   let text = SyntaxFacts.GetText(k)
                   select new Completion(text, text, null, imageSource, null);
        }

        private ImageSource GetImage(Symbol symbol)
        {
            var glyph = GetGlyph(symbol);
            return glyph == null ? null : _glyphService.GetGlyph(glyph.Value);
        }

        private static NQueryGlyph? GetGlyph(Symbol symbol)
        {
            switch (symbol.Kind)
            {
                case SymbolKind.Column:
                    return NQueryGlyph.Column;
                case SymbolKind.SchemaTable:
                    return NQueryGlyph.Table;
                case SymbolKind.DerivedTable:
                    return NQueryGlyph.Table;
                case SymbolKind.TableInstance:
                    return NQueryGlyph.TableRef;
                case SymbolKind.ColumnInstance:
                    return NQueryGlyph.Column;
                default:
                    return null;
            }
        }
    }
}