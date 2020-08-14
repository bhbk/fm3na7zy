
CREATE VIEW [svc].[uvw_UserMounts]
AS
SELECT        IdentityId, CredentialId, AuthType, ServerAddress, ServerShare, Enabled, Created, Immutable
FROM            dbo.tbl_UserMounts
