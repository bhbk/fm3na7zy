
CREATE VIEW [svc].[uvw_Credential]
AS
SELECT
	Id
	,Domain
	,UserName
	,EncryptedPass
	,IsEnabled
	,IsDeletable
	,CreatedUtc
	,LastUpdatedUtc

FROM
	[dbo].[tbl_Credential]
