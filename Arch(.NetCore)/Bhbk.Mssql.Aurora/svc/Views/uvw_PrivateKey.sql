
CREATE VIEW [svc].[uvw_PrivateKey]
AS
SELECT        Id, IdentityId, PublicKeyId, KeyValue, KeyAlgo, KeyPass, KeyFormat, IsEnabled, IsDeletable, CreatedUtc, LastUpdatedUtc
FROM            [dbo].[tbl_PrivateKey]
