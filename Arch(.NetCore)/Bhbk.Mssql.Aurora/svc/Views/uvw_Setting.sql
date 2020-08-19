
CREATE VIEW [svc].[uvw_Setting]
AS
SELECT        Id, IdentityId, ConfigKey, ConfigValue, Deletable, Created, LastUpdated
FROM            dbo.tbl_Setting
