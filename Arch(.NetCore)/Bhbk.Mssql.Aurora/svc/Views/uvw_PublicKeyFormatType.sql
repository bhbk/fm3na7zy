
CREATE VIEW [svc].[uvw_PublicKeyFormatType]

AS

SELECT Id
      ,Name
	  ,IsEnabled
	  ,IsEditable
	  ,IsDeletable
FROM [dbo].[tbl_PublicKeyFormatType]