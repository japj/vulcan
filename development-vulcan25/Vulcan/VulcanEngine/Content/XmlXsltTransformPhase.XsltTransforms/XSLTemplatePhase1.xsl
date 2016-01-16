<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:vulcan="http://tempuri.org/vulcan2.xsd">
  <xsl:param name="TemplatePath">placeholder</xsl:param>

  <xsl:template match="node()|@*">
    <xsl:copy>
      <xsl:apply-templates select="node()|@*"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="processing-instruction('vulcan-template')">
    <xsl:variable name="TemplateName" select="substring-before(substring-after(.,'TemplateName=&quot;'),'&quot;')"/>
    <xsl:variable name="TemplateData" select="document(concat($TemplatePath,'\',$TemplateName))/Template/*"/>
    <xsl:element name="__Template">
      <xsl:attribute name="Name">
        <xsl:value-of select="$TemplateName"/>
      </xsl:attribute>
      <xsl:element name="__Parameters">
        <xsl:call-template name="parseVulcanTemplatePI">
          <xsl:with-param name="templateName" select="$TemplateName"/>
          <xsl:with-param name="templateParameters" select="."/>
        </xsl:call-template>
      </xsl:element>
      <xsl:element name="__TemplateData">
        <xsl:apply-templates select="$TemplateData" />
      </xsl:element>
    </xsl:element>
  </xsl:template>
  
  <!-- 
  Transformation step to remove any xmlns="" references created by the document() load in the processing-instruction.
  When XSLT 2.0 is available in Vulcan we can use unparsed-text function.
  -->

  <xsl:template name="parseVulcanTemplatePI">
    <xsl:param name="templateName" />
    <xsl:param name="templateParameters" />
    <xsl:choose>
      <xsl:when test="contains($templateParameters,' ')">
        <xsl:variable name="token" select="substring-before($templateParameters,' ')" />
        
        <xsl:call-template name="printTemplateParameter">
          <xsl:with-param name="paramName" select="concat($templateName,'_',normalize-space(substring-before($token,'=')))" />
          <xsl:with-param name="paramValue" select="substring-before(substring-after(substring-after($token,'='),'&quot;'),'&quot;')"/>
        </xsl:call-template>
        
        <xsl:call-template name="parseVulcanTemplatePI">
          <xsl:with-param name="templateName" select="$templateName"/>
          <xsl:with-param name="templateParameters" select="substring-after($templateParameters,' ')"/>
        </xsl:call-template>
        
      </xsl:when>
      
      <xsl:otherwise>
        <xsl:variable name="token" select="$templateParameters" />

        <xsl:if test="contains($token,'=')">
        <xsl:call-template name="printTemplateParameter">
          <xsl:with-param name="paramName" select="concat($templateName,'_',normalize-space(substring-before($token,'=')))" />
          <xsl:with-param name="paramValue" select="substring-before(substring-after(substring-after($token,'='),'&quot;'),'&quot;')"/>
        </xsl:call-template>
        </xsl:if>
        
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="printTemplateParameter">
    <xsl:param name="paramName" />
    <xsl:param name="paramValue" />
    <xsl:element name="{$paramName}">
      <xsl:value-of select="$paramValue"/>
    </xsl:element>
  </xsl:template>
  
</xsl:stylesheet>