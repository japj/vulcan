<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:vulcan="http://schemas.microsoft.com/detego/2007/07/07/VulcanConfig.xsd">
  <xsl:param name="RootFile">mainfile.xml</xsl:param>
  <xsl:param name="XSLTFolderPath">placeholder</xsl:param>
  <xsl:variable name="TemplatePath" select="concat($XSLTFolderPath,'\Templates')" />

  <xsl:variable name="EscapeChar" select="'$'"/>
  <xsl:variable name="StartDelimiter" select="'('"/>
  <xsl:variable name="EndDelimiter" select="')'"/>
  <xsl:variable name="SpaceChar" select="' '"/>
  <xsl:variable name="EndBracket" select="']'"/>

  <xsl:template match="node()|@*">
    <xsl:copy>
      <xsl:apply-templates select="node()|@*"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="vulcan:Import">
    <xsl:apply-templates select="document(concat($TemplatePath,'\',translate(@File,'.','\'),'.xml'))"/>
  </xsl:template>

  <!-- Strip out <root> from templates -->
  <xsl:template match="vulcan:root">
    <xsl:apply-templates select="@*|node()"/>
  </xsl:template>
  

  <!-- Strip parameters from output XML since they are only used for the preprocessor -->
  <!-- TODO: How do we enforce reserved tags so that no pattern developer gets caught by this? -->
  <xsl:template match="Parameter" />

  <xsl:template match="@*">
    <xsl:attribute name="{name()}">
      <xsl:call-template name="TextReplace">
        <xsl:with-param name="outputString" select="."/>
      </xsl:call-template>
    </xsl:attribute>
  </xsl:template>

  <xsl:template match="text()">
    <xsl:call-template name="TextReplace">
      <xsl:with-param name="outputString" select="."/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="TextReplace">
    <xsl:param name="outputString"/>
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
            <xsl:copy-of select="document($RootFile)//parameter[@name=$VarName]/node()|@*"/>
            <xsl:call-template name="TextReplace">
              <xsl:with-param name="outputString" select="substring-after($outputString,concat($VarName,$EndDelimiter))"/>
            </xsl:call-template>
          </xsl:when>
		  <xsl:when test="starts-with($SubstringAfterEscape, $SpaceChar) or starts-with($SubstringAfterEscape, $EndBracket)">
			  <xsl:value-of select="$outputString"/>
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




  <!-- TODO: If we can find escape and delimiter characters that are compatible with XML parsing, then add in the following templates to also get parameter replacement on
	           node names.  Attribute names would need a similar addition.  Note that when we match node(), we have to override comment() and processing-instruction with 
			   their own templates.

	<xsl:template match="node()">
		<xsl:variable name="NodeName">
			<xsl:call-template name="TextReplace">
				<xsl:with-param name="outputString" select="name()"/>
			</xsl:call-template>
		</xsl:variable>
		<xsl:element name="{$NodeName}">
			<xsl:apply-templates select="@*|node()" />
		</xsl:element>
	</xsl:template>
	
	<xsl:template match="comment()|processing-instruction()">
		<xsl:copy>
			<xsl:apply-templates select="@*|node()" />
		</xsl:copy>
	</xsl:template>
	-->

</xsl:stylesheet>