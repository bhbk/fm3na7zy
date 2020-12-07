
CREATE VIEW [svc].[uvw_PrivateKey]
AS
SELECT
	Id
	,IdentityId
	,PublicKeyId
	,KeyValue
	,KeyAlgo
	,KeyFormat
	,EncryptedPass
	,IsEnabled
	,IsDeletable
	,CreatedUtc
	,LastUpdatedUtc

FROM
	[dbo].[tbl_PrivateKey]
