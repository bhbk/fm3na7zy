
CREATE     VIEW [svc].[uvw_Credentials]
AS
SELECT        Id, Domain, UserName, Password, Enabled, Created, Immutable
FROM            dbo.tbl_Credentials
