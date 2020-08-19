
CREATE VIEW svc.uvw_User
AS
SELECT        IdentityId, IdentityAlias, RequirePassword, RequirePublicKey, FileSystemType, FileSystemReadOnly, DebugLevel, Enabled, Deletable, Created, LastUpdated
FROM            dbo.tbl_User
