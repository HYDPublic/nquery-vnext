using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace NQuery.Language.VSEditor
{
    [Export(typeof(IViewTaggerProvider))]
    [TagType(typeof(IErrorTag))]
    [ContentType("NQuery")]
    internal sealed class NQuerySemanticErrorTaggerProvider : IViewTaggerProvider
    {
        [Import]
        public INQuerySemanticModelManagerService SemanticModelManagerService { get; set; }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            var semanticModelManager = SemanticModelManagerService.GetSemanticModelManager(buffer);
            return new NQuerySemanticErrorTagger(buffer, semanticModelManager) as ITagger<T>;
        }
    }
}