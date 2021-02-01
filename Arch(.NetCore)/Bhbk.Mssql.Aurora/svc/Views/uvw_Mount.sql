
CREATE VIEW [svc].[uvw_Mount]
AS
SELECT
	UserId
	,AmbassadorId
	,AuthType
	,UncPath
	,IsEnabled
	,IsDeletable
	,CreatedUtc
	,LastUpdatedUtc

FROM
	[dbo].[tbl_Mount]
