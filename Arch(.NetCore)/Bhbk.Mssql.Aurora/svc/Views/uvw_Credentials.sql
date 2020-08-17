
CREATE VIEW svc.uvw_Credentials
AS
SELECT        Id, Domain, UserName, Password, Enabled, Created, LastUpdated, Immutable
FROM            dbo.tbl_Credentials
