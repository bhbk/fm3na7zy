
CREATE VIEW [svc].[uvw_Settings]
AS
SELECT        Id, ConfigKey, ConfigValue, Created, Immutable
FROM            dbo.tbl_Settings