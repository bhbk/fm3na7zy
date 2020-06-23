
CREATE VIEW [svc].[uvw_SysCredentials]
AS
SELECT        Id, Domain, UserName, Password, Enabled, Created, Immutable
FROM            dbo.tbl_SysCredentials