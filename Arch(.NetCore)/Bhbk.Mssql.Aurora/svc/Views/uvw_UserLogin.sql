
CREATE VIEW [svc].[uvw_UserLogin]
AS
SELECT
	IdentityId
	,IdentityAlias
	,FileSystemType
	,FileSystemChrootPath
	,IsPasswordRequired
	,IsPublicKeyRequired
	,IsFileSystemReadOnly
	,QuotaInBytes
	,QuotaUsedInBytes
	,SessionMax
	,SessionsInUse
	,Debugger
	,IsEnabled
	,IsDeletable
	,CreatedUtc
	,LastUpdatedUtc

FROM
	[dbo].[tbl_UserLogin]