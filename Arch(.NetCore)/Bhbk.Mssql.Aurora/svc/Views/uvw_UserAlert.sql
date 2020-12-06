
CREATE   VIEW [svc].[uvw_UserAlert]
AS
SELECT
	Id
	,IdentityId
	,OnDelete
	,OnDownload
	,OnUpload
	,ToDisplayName
	,ToEmailAddress
	,ToPhoneNumber
	,IsEnabled
	,CreatedUtc
	,LastUpdatedUtc

FROM
	[dbo].[tbl_UserAlert]
