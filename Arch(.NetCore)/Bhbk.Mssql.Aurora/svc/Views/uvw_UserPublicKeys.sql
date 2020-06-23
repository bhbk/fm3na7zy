
CREATE VIEW [svc].[uvw_UserPublicKeys]
AS
SELECT        Id, UserId, PrivateKeyId, KeyValueBase64, KeyValueAlgo, KeySig, KeySigAlgo, Hostname, Enabled, Created, Immutable
FROM            dbo.tbl_UserPublicKeys