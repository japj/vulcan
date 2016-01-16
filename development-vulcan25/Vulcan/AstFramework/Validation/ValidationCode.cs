namespace AstFramework.Validation
{
    public enum ValidationCode
    {
        ErrorInvalidName,
        ErrorRequiredPropertyNotSpecified,
        ErrorInvalidStringLength,
        ErrorInvalidBinaryLength,
        ErrorInvalidDecimalPrecision,
        ErrorInvalidDecimalScale,
        ErrorDuplicateName,
        ErrorDuplicateMeasureGroupName,
        ErrorCustomTypeNotSpecified,
        ErrorInvalidHashedKeyColumnIsNullable,
        ErrorInvalidStaticSourceValue,
        ErrorInvalidStaticSourceCount,
        ErrorInvalidDefaultValue,
        ErrorNullableKeyColumn,

        WarningNonDefaultLength,
        WarningNonDefaultPrecision,
        WarningNonDefaultScale,
        WarningNonemptyCustomType,
        WarningNonUnicodeString,
        WarningNonemptyDefault,
        WarningNonemptyComputed,
        WarningDuplicateKeyStructures,
        WarningDuplicateIndexStructures,
        WarningDuplicateRelationshipStructures,
        WarningDuplicateHierarchyStructures,
        WarningInt32PrimaryKeyColumn,
        WarningMultipleIdentityKeyColumns,
    }
}
