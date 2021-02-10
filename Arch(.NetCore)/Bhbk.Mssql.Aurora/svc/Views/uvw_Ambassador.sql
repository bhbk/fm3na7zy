
CREATE VIEW [svc].[uvw_Ambassador]
AS
SELECT
	Id
	,UserName
	,EncryptedPass
	,IsEnabled
	,IsDeletable
	,CreatedUtc

FROM
	[dbo].[tbl_Ambassador]
