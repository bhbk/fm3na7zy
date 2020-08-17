
CREATE VIEW svc.uvw_Users
AS
SELECT        IdentityId, IdentityAlias, RequirePassword, RequirePublicKey, FileSystemType, FileSystemReadOnly, DebugLevel, Enabled, Created, Immutable
FROM            dbo.tbl_Users
