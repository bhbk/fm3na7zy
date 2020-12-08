
CREATE VIEW [svc].[uvw_Mount]
AS
SELECT
	UserId
	,AmbassadorId
	,AuthType
	,ServerAddress
	,ServerShare
	,IsEnabled
	,IsDeletable
	,CreatedUtc
	,LastUpdatedUtc

FROM
	[dbo].[tbl_Mount]
