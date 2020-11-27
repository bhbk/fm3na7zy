


CREATE VIEW [svc].[uvw_Credential]
AS
SELECT
	Id
	,Domain
	,UserName
	,EncryptedPassword
	,IsEnabled
	,IsDeletable
	,CreatedUtc
	,LastUpdatedUtc

FROM
	[dbo].[tbl_Credential]
