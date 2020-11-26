
CREATE VIEW [svc].[uvw_UserMount]
AS
SELECT
	IdentityId
	,CredentialId
	,AuthType
	,ServerAddress
	,ServerShare
	,IsEnabled
	,IsDeletable
	,CreatedUtc
	,LastUpdatedUtc

FROM
	[dbo].[tbl_UserMount]
