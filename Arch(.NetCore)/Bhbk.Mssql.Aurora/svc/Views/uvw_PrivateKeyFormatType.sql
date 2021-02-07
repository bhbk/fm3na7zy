
CREATE VIEW [svc].[uvw_PrivateKeyFormatType]

AS

SELECT Id
      ,Name
	  ,IsEnabled
	  ,IsEditable
	  ,IsDeletable
FROM [dbo].[tbl_PrivateKeyFormatType]