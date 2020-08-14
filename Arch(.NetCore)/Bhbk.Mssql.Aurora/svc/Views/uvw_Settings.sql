
CREATE VIEW [svc].[uvw_Settings]
AS
SELECT        Id, IdentityId, ConfigKey, ConfigValue, Created, Immutable
FROM            dbo.tbl_Settings
