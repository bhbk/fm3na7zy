
CREATE VIEW [svc].[uvw_Ambassadors]
AS
SELECT        Id, Domain, UserName, Password, Enabled, Created, Immutable
FROM            dbo.tbl_Ambassadors