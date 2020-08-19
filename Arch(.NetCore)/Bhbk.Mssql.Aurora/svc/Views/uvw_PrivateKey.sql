
CREATE VIEW [svc].[uvw_PrivateKey]
AS
SELECT        Id, IdentityId, PublicKeyId, KeyValue, KeyAlgo, KeyPass, KeyFormat, Enabled, Deletable, Created, LastUpdated
FROM            dbo.tbl_PrivateKey
