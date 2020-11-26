
CREATE VIEW [svc].[uvw_Credential]
AS
SELECT
	Id
	,Domain
	,UserName
	,Password
	,IsEnabled
	,IsDeletable
	,CreatedUtc
	,LastUpdatedUtc

FROM
	[dbo].[tbl_Credential]
