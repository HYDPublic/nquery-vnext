﻿using System.Linq;

using Xunit;

using NQuery.Authoring.QuickInfo;
using NQuery.Authoring.QuickInfo.Providers;
using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery.Authoring.UnitTests.QuickInfo.Providers
{
    public class WildcardSelectColumnQuickInfoModelProviderTests : QuickInfoModelProviderTests
    {
        protected override IQuickInfoModelProvider CreateProvider()
        {
            return new WildcardSelectColumnQuickInfoModelProvider();
        }

        protected override QuickInfoModel CreateExpectedModel(SemanticModel semanticModel)
        {
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var syntax = syntaxTree.Root.DescendantNodes().OfType<WildcardSelectColumnSyntax>().Single();
            var span = syntax.TableName.Span;
            var symbol = semanticModel.GetTableInstance(syntax);
            var markup = SymbolMarkup.ForSymbol(symbol);
            return new QuickInfoModel(semanticModel, span, NQueryGlyph.TableInstance, markup);
        }

        [Fact]
        public void WildcardSelectColumnQuickInfoModelProvider_MatchesInAlias()
        {
            var query = @"
                SELECT  {e}.*
                FROM    Employees e
             ";

            AssertIsMatch(query);
        }

        [Fact]
        public void WildcardSelectColumnQuickInfoModelProvider_DoesNotMatchesUnresolved()
        {
            var query = @"
                SELECT  {x}.*
                FROM    Employees e
            ";

            AssertIsNotMatch(query);
        }

        [Fact]
        public void WildcardSelectColumnQuickInfoModelProvider_DoesNotMatchAfterDot()
        {
            var query = @"
                SELECT  e.{*}
                FROM    Employees e
            ";

            AssertIsNotMatch(query);
        }
    }
}