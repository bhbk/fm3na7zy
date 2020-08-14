
CREATE VIEW svc.uvw_Users
AS
SELECT        IdentityId, IdentityAlias, AllowPassword, FileSystemType, FileSystemReadOnly, DebugLevel, Enabled, Created, Immutable
FROM            dbo.tbl_Users
