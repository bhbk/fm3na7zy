
CREATE VIEW [svc].[uvw_UserMounts]
AS
SELECT        UserId, CredentialId, AuthType, ServerName, ServerPath, Enabled, Created, Immutable
FROM            dbo.tbl_UserMounts