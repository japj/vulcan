using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VulcanEngine.IR.Ast.Ssas
{
    public enum SsasDataMimeType
    {
        image_gif,
        image_jpeg,
    }

    public enum SsasDataFormat
    {
    }

    public enum SsasCollation
    {
    }

    public enum SsasInvalidXMLCharacterProcessing
    {
        Preserve,
        Remove,
        Replace,
    }

    public enum SsasNullProcessing
    {
        Error,
        ZeroOrBlank,
        Automatic,
    }

    public enum SsasMeasureFormat
    {
        Standard,
        Currency,
        Percent,
        ShortDate,
        LongDate,
        // TODO: How can we do arbitrary format strings?
    }

    public enum SsasTrimming
    {
        Left,
        Right,
        LeftRight,
        None,
    }

    public enum SsasAggregationFunction
    {
        Sum,
        Count,
        Min,
        Max,
        DistinctCount,
        None,
        ByAccount,
        AverageOfChildren,
        FirstChild,
        LastChild,
        FirstNonEmpty,
        LastNonEmpty,
    }

    public enum SsasOptimizedState
    {
        FullyOptimized,
        NotOptimized,
    }

    public enum SsasAggregationUsage
    {
        Full,
        None,
        Unrestricted,
        Default,
    }

    public enum SsasHierarchyUniqueNameStyle
    {
        IncludeDimensionName,
        ExcludeDimensionName,
    }

    public enum SsasMemberUniqueNameStyle
    {
        Native,
        NamePath,
    }

    public enum SsasErrorConfiguration
    {
        Default,
        Custom,
    }

    public enum SsasProcessingMode
    {
        Regular,
        LazyAggregations,
    }

    public enum SsasStorageMode
    {
        Rolap,
        Molap,
        Holap,
    }

    public enum SsasMeasureGroupType
    {
        Regular,
        ExchangeRate,
        Sales,
        Budget,
        FinancialReporting,
        Marketing,
        Inventory,
    }

    public enum SsasDataAggregation
    {
        None,
        DataAggregatable,
        CacheAggregatable,
        DataAndCacheAggregatable,
    }

    public enum SsasOrderBy
    {
        Key,
        Name,
        AttributeKey,
        AttributeName,
    }

    public enum SsasAttributeUsage
    {
        Regular,
        Key,
        Parent,
    }

    // TODO: Can we automatically generate our enumerations or our facets, so we don't have to author twice and keep in sync?
    // TODO: Maybe a validation tool that ensures everything is in sync rather than hitting runtime errors?
    // TODO: Can we automatically map enum matching failures to Other?  How do we get the value in that case?
    /*
    public enum SSASColumnDataFormat
    {
			// Predefined Numeric Formats
			GeneralNumber,
			G,
			g,
			Currency,
			C,
			c,
			Fixed,
			F,
			f,
			Standard,
			N,
			n,
			Percent,
			P,
			p,
			Scientific,
			E,
			e,
			D,
			d,
			X,
			x,
			Yes,
			No,
			True,
			False,
			On,
			Off,
			// TODO: Add RegEx pattern for user-defined numeric formats

			// Predefined Date Formats
			GeneralDate,
			G,
			LongDate,
			MediumDate,
			D,
			ShortDate,
			d,
			LongTime,
			MediumTime,
			T,
			ShortTime,
			t,
			f,
			F,
			g,
			M,
			m,
			R,
			r,
			s,
			u,
			U,
			Y,
			y,
			// TODO: Add RegEx pattern for user-defined date formats
    }*/
    /*
    <xs:attribute name="Collation" type="SSASColumnCollationFacet" />
		<xs:attribute name="Trimming" type="xs:boolean" default="false" />
		<xs:attribute name="InvalidXMLCharacterProcessing" type="SSASColumnInvalidXmlCharacterProcessingFacet" default="Preserved" />
		<xs:attribute name="NullProcessing" type="SSASColumnNullProcessingFacet" default="NotPermitted" />

     */
}