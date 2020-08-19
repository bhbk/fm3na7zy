
CREATE VIEW [svc].[uvw_UserMount]
AS
SELECT        IdentityId, CredentialId, AuthType, ServerAddress, ServerShare, Enabled, Deletable, Created, LastUpdated
FROM            dbo.tbl_UserMount
