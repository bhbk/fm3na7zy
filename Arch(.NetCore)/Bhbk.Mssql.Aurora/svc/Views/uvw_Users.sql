
CREATE VIEW svc.uvw_Users
AS
SELECT        Id, UserName, AllowPassword, FileSystemType, FileSystemReadOnly, DebugLevel, Enabled, Created, Immutable
FROM            dbo.tbl_Users
