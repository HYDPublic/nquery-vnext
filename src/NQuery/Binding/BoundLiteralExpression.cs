using System;

namespace NQuery.Binding
{
    internal sealed class BoundLiteralExpression : BoundExpression
    {
        private readonly object _value;

        public BoundLiteralExpression(object value)
        {
            _value = value;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.LiteralExpression; }
        }

        public override Type Type
        {
            get
            {
                return _value == null
                           ? TypeFacts.Null
                           : _value.GetType();
            }
        }

        public object Value
        {
            get { return _value; }
        }

        public override string ToString()
        {
            if (_value == null)
                return "NULL";

            if (_value is string)
                return $"'{_value}'"; // TODO: We should escape this

            if (_value is DateTime)
                return $"#{_value}#";

            return _value.ToString();
        }
    }
}