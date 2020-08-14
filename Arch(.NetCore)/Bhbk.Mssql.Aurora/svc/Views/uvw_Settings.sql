
CREATE VIEW [svc].[uvw_Settings]
AS
SELECT        Id, UserId, ConfigKey, ConfigValue, Created, Immutable
FROM            dbo.tbl_Settings
