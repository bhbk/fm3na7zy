
CREATE PROCEDURE [svc].[usp_SmbAuthType_Insert]
	@Id int
   ,@Name nvarchar(32)
   ,@Description nvarchar(256)
   ,@IsEnabled bit
   ,@IsEditable bit
   ,@IsDeletable bit

AS

SET NOCOUNT ON;

INSERT INTO [dbo].[tbl_SmbAuthType] 
	(Id
	,Name
    ,IsEnabled
    ,Description
	,IsEditable
    ,IsDeletable
    )
VALUES 
	(@Id
    ,@Name
    ,@Description
    ,@IsEnabled
	,@IsEditable
    ,@IsDeletable
    );

    /*  Select all entity values to return
        ----------------------------------------------------
       */
	
SELECT * 
FROM [dbo].[tbl_SmbAuthType] 
WHERE Id = @Id