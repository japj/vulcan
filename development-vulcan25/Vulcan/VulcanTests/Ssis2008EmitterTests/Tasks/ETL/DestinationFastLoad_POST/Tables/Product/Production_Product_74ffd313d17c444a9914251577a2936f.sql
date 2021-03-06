SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
GO

-------------------------------------------------------------------
IF EXISTS (SELECT * from sys.objects WHERE object_id = OBJECT_ID(N'[Production].[Product]') AND type IN (N'U'))
DROP TABLE [Production].[Product]
GO

CREATE TABLE [Production].[Product]
(
-- Columns Definition
	[ProductID] bigint
,	[VersionNumber] rowversion NOT NULL

-- Constraints

)
ON [PRIMARY]
WITH (DATA_COMPRESSION = NONE)
GO

-------------------------------------------------------------------


