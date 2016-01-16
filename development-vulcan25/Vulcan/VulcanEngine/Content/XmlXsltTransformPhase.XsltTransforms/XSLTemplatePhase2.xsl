<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:vulcan="http://tempuri.org/vulcan2.xsd">
  <xsl:param name="TemplatePath">placeholder</xsl:param>

  <xsl:variable name="EscapeChar" select="'$'"/>
  <xsl:variable name="StartDelimiter" select="'('"/>
  <xsl:variable name="EndDelimiter" select="')'"/>

  <xsl:template match="node()|@*">
    <xsl:copy>
      <xsl:apply-templates select="node()|@*"/>
    </xsl:copy>
  </xsl:template>


  <xsl:template match="//__Template">
    <xsl:variable name="templateName" select="@Name" />
    <xsl:for-each select="__TemplateData">
      <xsl:call-template name="CopyChildren">
        <xsl:with-param name="templateName" select="$templateName"/>
      </xsl:call-template>        
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="CopyChildren">
    <xsl:param name="templateName"/>
    
    <xsl:for-each select="*">
      <xsl:element name="{local-name()}" namespace="http://tempuri.org/vulcan2.xsd">
        <xsl:for-each select="@*">
          <xsl:attribute name="{local-name()}">
            <xsl:call-template name="TextReplace">
              <xsl:with-param name="outputString" select="."/>
              <xsl:with-param name="templateName" select="$templateName"/>
            </xsl:call-template>
          </xsl:attribute>
        </xsl:for-each>

        <xsl:call-template name="CopyChildren">
          <xsl:with-param name="templateName" select="$templateName"/>
        </xsl:call-template>

        <xsl:for-each select="text()">
          <xsl:call-template name="TextReplace">
            <xsl:with-param name="outputString" select="."/>
            <xsl:with-param name="templateName" select="$templateName"/>
          </xsl:call-template>
        </xsl:for-each>
      </xsl:element>
    </xsl:for-each>
  </xsl:template>

  <!-- 
  Transformation step to remove any xmlns="" references created by the document() load in the processing-instruction.
  When XSLT 2.0 is available in Vulcan we can use unparsed-text function.
  -->
  
  <!-- Strip parameters from output XML since they are only used for the preprocessor -->
  <!-- TODO: How do we enforce reserved tags so that no pattern developer gets caught by this? -->
  

  <xsl:template name="TextReplace">
    <xsl:param name="outputString"/>
    <xsl:param name="templateName" />
    <xsl:choose>
      <xsl:when test="$outputString and contains($outputString,$EscapeChar)">
        <xsl:variable name="SubstringAfterEscape" select="substring-after($outputString,$EscapeChar)"/>
        <xsl:choose>
          <xsl:when test="starts-with($SubstringAfterEscape,$EscapeChar)">
            <xsl:value-of select="concat(substring-before($outputString,$EscapeChar),$EscapeChar)" />
            <xsl:call-template name="TextReplace">
              <xsl:with-param name="outputString" select="substring($SubstringAfterEscape,2)"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="starts-with($SubstringAfterEscape,$StartDelimiter) and contains($SubstringAfterEscape,$EndDelimiter)">
            <xsl:variable name="VarName" select="substring-before(substring-after($SubstringAfterEscape,$StartDelimiter),$EndDelimiter)"/>
            <xsl:value-of select="substring-before($outputString,$EscapeChar)" />
            <xsl:copy-of select="//*[local-name() = concat($templateName,'_',$VarName)]/node()|@*"/>
            <xsl:call-template name="TextReplace">
              <xsl:with-param name="outputString" select="substring-after($outputString,concat($VarName,$EndDelimiter))"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <xsl:message terminate="yes">
              Encountered invalid parameter escaping syntax:
              <xsl:value-of select="concat($EscapeChar,$SubstringAfterEscape)"/>
            </xsl:message>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$outputString"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

</xsl:stylesheet>