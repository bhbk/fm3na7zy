
CREATE VIEW [svc].[uvw_LoginDebugType]

AS

SELECT Id
      ,Name
	  ,IsEnabled
	  ,IsEditable
	  ,IsDeletable
FROM [dbo].[tbl_LoginDebugType]