
CREATE VIEW [svc].[uvw_PrivateKeys]
AS
SELECT        Id, IdentityId, PublicKeyId, KeyValue, KeyAlgo, KeyPass, KeyFormat, Enabled, Created, LastUpdated, Immutable
FROM            dbo.tbl_PrivateKeys
