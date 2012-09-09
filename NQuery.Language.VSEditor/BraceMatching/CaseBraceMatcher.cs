using System.ComponentModel.Composition;

namespace NQuery.Language.VSEditor.BraceMatching
{
    [Export(typeof(IBraceMatcher))]
    internal sealed class CaseBraceMatcher : PairedTokenBraceMatcher
    {
        public CaseBraceMatcher()
            : base(SyntaxKind.CaseKeyword, SyntaxKind.EndKeyword)
        {
        }
    }
}