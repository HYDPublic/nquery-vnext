using System;
using System.Text;
using System.Windows.Media;

using ActiproSoftware.Text;
using ActiproSoftware.Windows.Controls.SyntaxEditor.Highlighting;
using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt.Implementation;

using NQuery.Authoring.ActiproWpf.Classification;
using NQuery.Symbols;

namespace NQuery.Authoring.ActiproWpf.SymbolContent
{
    internal static class HtmlMarkupEmitter
    {
        public static string GetHtml(NQueryGlyph glyph, SymbolMarkup symbolMarkup, INQueryClassificationTypes classificationTypes, IHighlightingStyleRegistry highlightingStyleRegistry)
        {
            var sb = new StringBuilder();
            sb.AppendGlyph(glyph);
            sb.AppendMarkup(symbolMarkup, classificationTypes, highlightingStyleRegistry);
            return sb.ToString();
        }

        private static void AppendGlyph(this StringBuilder sb, NQueryGlyph glyph)
        {
            sb.Append("<img src=\"");
            sb.Append(glyph);
            sb.Append("\" align=\"absbottom\" />");
        }

        private static void AppendMarkup(this StringBuilder sb, SymbolMarkup symbolMarkup, INQueryClassificationTypes classificationTypes, IHighlightingStyleRegistry highlightingStyleRegistry)
        {
            foreach (var node in symbolMarkup.Nodes)
                sb.AppendNode(node, classificationTypes, highlightingStyleRegistry);
        }

        private static void AppendNode(this StringBuilder sb, SymbolMarkupNode node, INQueryClassificationTypes classificationTypes, IHighlightingStyleRegistry highlightingStyleRegistry)
        {
            var classificationType = GetClassificationType(node.Kind, classificationTypes);
            sb.AppendText(node.Text, classificationType, highlightingStyleRegistry);
        }

        private static IClassificationType GetClassificationType(SymbolMarkupKind kind, INQueryClassificationTypes classificationTypes)
        {
            switch (kind)
            {
                case SymbolMarkupKind.Whitespace:
                    return classificationTypes.WhiteSpace;
                case SymbolMarkupKind.Punctuation:
                    return classificationTypes.Punctuation;
                case SymbolMarkupKind.Keyword:
                    return classificationTypes.Keyword;
                case SymbolMarkupKind.TableName:
                    return classificationTypes.SchemaTable;
                case SymbolMarkupKind.DerivedTableName:
                    return classificationTypes.DerivedTable;
                case SymbolMarkupKind.CommonTableExpressionName:
                    return classificationTypes.CommonTableExpression;
                case SymbolMarkupKind.ColumnName:
                    return classificationTypes.Column;
                case SymbolMarkupKind.VariableName:
                    return classificationTypes.Variable;
                case SymbolMarkupKind.ParameterName:
                    return classificationTypes.Identifier;
                case SymbolMarkupKind.FunctionName:
                    return classificationTypes.Function;
                case SymbolMarkupKind.AggregateName:
                    return classificationTypes.Aggregate;
                case SymbolMarkupKind.MethodName:
                    return classificationTypes.Method;
                case SymbolMarkupKind.PropertyName:
                    return classificationTypes.Property;
                case SymbolMarkupKind.TypeName:
                    return classificationTypes.Identifier;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        private static void AppendText(this StringBuilder sb, string text, IClassificationType classificationType, IHighlightingStyleRegistry highlightingStyleRegistry)
        {
            var highlightingStyle = highlightingStyleRegistry[classificationType];
            var styleBuilder = new StringBuilder();
            styleBuilder.AppendStyle(highlightingStyle);
            var hasStyle = styleBuilder.Length > 0;

            if (hasStyle)
            {
                sb.Append("<span style=\"");
                sb.Append(styleBuilder);
                sb.Append("\">");
            }

            sb.Append(HtmlContentProvider.Escape(text));

            if (hasStyle)
                sb.Append("</span>");
        }

        private static void AppendStyle(this StringBuilder sb, IHighlightingStyle highlightingStyle)
        {
            sb.AppendColor("background-color", highlightingStyle.Background);
            sb.AppendColor("color", highlightingStyle.Foreground);
            sb.AppendFontFamiliy("font-family", highlightingStyle.FontFamilyName);
            sb.AppendFontSize("font-size", highlightingStyle.FontSize);
            sb.AppendFontWeight("font-weight", highlightingStyle.Bold);
            sb.AppendFontStyle("font-style", highlightingStyle.Italic);
            sb.AppendTextDecoration("text-decoration", highlightingStyle.UnderlineStyle);
        }

        private static void AppendFontFamiliy(this StringBuilder sb, string key, string fontFamilyName)
        {
            if (string.IsNullOrEmpty(fontFamilyName))
                return;

            sb.AppendKeyValue(key, fontFamilyName);
        }

        private static void AppendColor(this StringBuilder sb, string key, Brush brush)
        {
            var solidColorBrush = brush as SolidColorBrush;
            if (solidColorBrush == null || solidColorBrush.Color == Colors.Black)
                return;

            var value = solidColorBrush.Color.ToString();
            sb.AppendKeyValue(key, value);
        }

        private static void AppendFontSize(this StringBuilder sb, string key, double size)
        {
            if (size == 0.0)
                return;

            var value = size.ToString();
            sb.AppendKeyValue(key, value);
        }

        private static void AppendFontWeight(this StringBuilder sb, string key, bool? isBold)
        {
            if (isBold == null)
                return;

            var value = isBold.Value ? "bold" : "normal";
            sb.AppendKeyValue(key, value);
        }

        private static void AppendFontStyle(this StringBuilder sb, string key, bool? isItalic)
        {
            if (isItalic == null)
                return;

            var value = isItalic.Value ? "italic" : "normal";
            sb.AppendKeyValue(key, value);
        }

        private static void AppendTextDecoration(this StringBuilder sb, string key, HighlightingStyleLineStyle underlineStyle)
        {
            var hasUnderlineStyle = underlineStyle != HighlightingStyleLineStyle.None &&
                                    underlineStyle != HighlightingStyleLineStyle.Default;

            if (!hasUnderlineStyle)
                return;

            sb.AppendKeyValue(key, "underline");
        }

        private static void AppendKeyValue(this StringBuilder sb, string name, string value)
        {
            sb.Append(name);
            sb.Append(": ");
            sb.Append(value);
            sb.Append(";");
        }
    }
}