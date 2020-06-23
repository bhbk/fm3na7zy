
CREATE VIEW [svc].[uvw_Users]
AS
SELECT        Id, IdentityId, UserName, FileSystemType, FileSystemReadOnly, DebugLevel, Enabled, Created, Immutable
FROM            dbo.tbl_Users