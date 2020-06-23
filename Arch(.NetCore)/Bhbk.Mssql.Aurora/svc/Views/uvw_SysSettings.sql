
CREATE VIEW [svc].[uvw_SysSettings]
AS
SELECT        Id, ConfigKey, ConfigValue, Created, Immutable
FROM            dbo.tbl_SysSettings