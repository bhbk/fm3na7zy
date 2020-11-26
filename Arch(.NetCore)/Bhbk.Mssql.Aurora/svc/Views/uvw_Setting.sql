
CREATE VIEW [svc].[uvw_Setting]
AS
SELECT
	Id
	,IdentityId
	,ConfigKey
	,ConfigValue
	,IsDeletable
	,CreatedUtc
	,LastUpdatedUtc

FROM
	[dbo].[tbl_Setting]
