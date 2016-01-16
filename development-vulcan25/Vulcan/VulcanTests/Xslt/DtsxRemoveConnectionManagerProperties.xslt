<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:DTS="www.microsoft.com/SqlServer/Dts">
  <xsl:template match="@*|node()|text()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()|text()"/>
    </xsl:copy>
  </xsl:template>

  <!--Removes ConnectionString property for FILE Connection Members-->
  <xsl:template match="//DTS:ConnectionManager[//DTS:Property[@DTS:Name='CreationName']='FILE']//DTS:ObjectData//DTS:ConnectionManager//DTS:Property[@DTS:Name='ConnectionString']"/>
  
</xsl:stylesheet>


