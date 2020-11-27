
CREATE VIEW [svc].[uvw_User]
AS
SELECT
	IdentityId
	,IdentityAlias
	,FileSystemType
	,IsPasswordRequired
	,IsPublicKeyRequired
	,IsFileSystemReadOnly
	,QuotaInBytes
	,QuotaUsedInBytes
	,Debugger
	,IsEnabled
	,IsDeletable
	,CreatedUtc
	,LastUpdatedUtc

FROM
	[dbo].[tbl_User]
