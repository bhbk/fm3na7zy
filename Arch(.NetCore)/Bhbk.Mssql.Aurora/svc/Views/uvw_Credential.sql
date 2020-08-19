
CREATE VIEW svc.uvw_Credential
AS
SELECT        Id, Domain, UserName, Password, Enabled, Deletable, Created, LastUpdated
FROM            dbo.tbl_Credential
