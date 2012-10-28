using System;
using System.Linq;

using NQuery.Binding;

namespace NQuery.BoundNodes
{
    internal sealed class BoundAllAnySubselect : BoundExpression
    {
        private readonly BoundQuery _boundQuery;
        private readonly Type _type;

        public BoundAllAnySubselect(BoundExpression left, BoundQuery boundQuery)
        {
            _boundQuery = boundQuery;
            var firstColumn = boundQuery.SelectColumns.FirstOrDefault();
            _type = firstColumn == null
                        ? KnownTypes.Unknown
                        : firstColumn.Expression.Type;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.AllAnySubselect; }
        }

        public BoundQuery BoundQuery
        {
            get { return _boundQuery; }
        }

        public override Type Type
        {
            get { return _type; }
        }
    }
}