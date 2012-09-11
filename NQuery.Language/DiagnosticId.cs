using System;

namespace NQuery.Language
{
    public enum DiagnosticId
    {
        InternalError,

        IllegalInputCharacter,
        UnterminatedComment,
        UnterminatedString,
        UnterminatedQuotedIdentifier,
        UnterminatedParenthesizedIdentifier,
        UnterminatedDate,

        InvalidDate,
        InvalidInteger,
        InvalidReal,
        InvalidBinary,
        InvalidOctal,
        InvalidHex,
        InvalidTypeReference,
        NumberTooLarge,
        TokenExpected,
        SimpleExpressionExpected,
        TableReferenceExpected,
        InvalidOperatorForAllAny,

        UndeclaredTable,
        UndeclaredVariable,
        UndeclaredFunction,
        UndeclaredMethod,
        UndeclaredColumn,
        UndeclaredProperty,
        UndeclaredType,
        UndeclaredEntity,
        AmbiguousReference,
        AmbiguousTableRef,
        AmbiguousColumnRef,
        AmbiguousTable,
        AmbiguousConstant,
        AmbiguousParameter,
        AmbiguousAggregate,
        AmbiguousProperty,
        AmbiguousType,
        AmbiguousInvocation,
        InvocationRequiresParenthesis,
        CannotApplyUnaryOperator,
        AmbiguousUnaryOperator,
        CannotApplyBinaryOperator,
        AmbiguousOperatorOverloading,
        AmbiguousBinaryOperator,
        AmbiguousCastingOperator,
        AsteriskModifierNotAllowed,
        WhenMustEvaluateToBoolIfCaseInputIsOmitted,
        CannotLoadTypeAssembly,
        CannotFoldConstants,
        CannotConvert,

        MustSpecifyTableToSelectFrom,
        AggregateCannotContainAggregate,
        AggregateCannotContainSubquery,
        AggregateDoesNotSupportType,
        AggregateInWhere,
        AggregateInOn,
        AggregateInGroupBy,
        AggregateContainsColumnsFromDifferentQueries,
        AggregateInvalidInCurrentContext,
        DuplicateTableRefInFrom,
        TableRefInaccessible,
        TopWithTiesRequiresOrderBy,
        OrderByColumnPositionIsOutOfRange,
        WhereClauseMustEvaluateToBool,
        HavingClauseMustEvaluateToBool,
        SelectExpressionNotAggregatedAndNoGroupBy,
        SelectExpressionNotAggregatedOrGrouped,
        HavingExpressionNotAggregatedOrGrouped,
        OrderByExpressionNotAggregatedAndNoGroupBy,
        OrderByExpressionNotAggregatedOrGrouped,
        OrderByInvalidInSubqueryUnlessTopIsAlsoSpecified,
        InvalidDataTypeInSelectDistinct,
        InvalidDataTypeInGroupBy,
        InvalidDataTypeInOrderBy,
        InvalidDataTypeInUnion,
        DifferentExpressionCountInBinaryQuery,
        OrderByItemsMustBeInSelectListIfUnionSpecified,
        OrderByItemsMustBeInSelectListIfDistinctSpecified,
        GroupByItemDoesNotReferenceAnyColumns,
        ConstantExpressionInOrderBy,
        TooManyExpressionsInSelectListOfSubquery,
        InvalidRowReference,
        NoColumnAliasSpecified,
        CteHasMoreColumnsThanSpecified,
        CteHasFewerColumnsThanSpecified,
        CteHasDuplicateColumnName,
        CteHasDuplicateTableName,
        CteDoesNotHaveUnionAll,
        CteDoesNotHaveAnchorMember,
        CteContainsRecursiveReferenceInSubquery,
        CteContainsUnexpectedAnchorMember,
        CteContainsMultipleRecursiveReferences,
        CteContainsUnion,
        CteContainsDistinct,
        CteContainsTop,
        CteContainsOuterJoin,
        CteContainsGroupByHavingOrAggregate,
        CteHasTypeMismatchBetweenAnchorAndRecursivePart
    }
}