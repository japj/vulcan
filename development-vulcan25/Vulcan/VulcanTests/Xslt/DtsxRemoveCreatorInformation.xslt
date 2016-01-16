<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:DTS="www.microsoft.com/SqlServer/Dts"  xmlns:SQLTask="www.microsoft.com/sqlserver/dts/tasks/sqltask">
  <xsl:template match="@*|node()|text()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()|text()"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="//DTS:Property[@DTS:Name='CreationDate']" />
  <xsl:template match="//DTS:Property[@DTS:Name='CreatorName']" />
  <xsl:template match="//DTS:Property[@DTS:Name='CreatorComputerName']" />

  <!--The logging query statements include the computer name and creator name-->
  <xsl:template match="//DTS:EventHandler//DTS:Executable[DTS:Property[@DTS:Name='ObjectName']='Exec usp_PackageError']//DTS:ObjectData//SQLTask:SqlTaskData/@SQLTask:SqlStatementSource"/>
  <xsl:template match="//DTS:EventHandler//DTS:Executable[DTS:Property[@DTS:Name='ObjectName']='Exec usp_PackageEnd']//DTS:ObjectData//SQLTask:SqlTaskData/@SQLTask:SqlStatementSource"/>
  <xsl:template match="//DTS:EventHandler//DTS:Executable[DTS:Property[@DTS:Name='ObjectName']='Exec usp_PackageStart']//DTS:ObjectData//SQLTask:SqlTaskData/@SQLTask:SqlStatementSource"/>
  
</xsl:stylesheet>


