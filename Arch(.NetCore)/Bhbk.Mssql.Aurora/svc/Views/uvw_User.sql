﻿
CREATE VIEW [svc].[uvw_User]
AS
SELECT
	IdentityId
	,IdentityAlias
	,RequirePassword
	,RequirePublicKey
	,FileSystemType
	,FileSystemReadOnly
	,DebugLevel
	,IsEnabled
	,IsDeletable
	,CreatedUtc
	,LastUpdatedUtc

FROM
	[dbo].[tbl_User]
