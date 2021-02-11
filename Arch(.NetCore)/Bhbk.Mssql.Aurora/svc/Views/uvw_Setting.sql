CREATE VIEW [svc].[uvw_Setting]
AS
SELECT Id,
	UserId,
	ConfigKey,
	ConfigValue,
	IsDeletable,
	CreatedUtc
FROM [dbo].[tbl_Setting]
