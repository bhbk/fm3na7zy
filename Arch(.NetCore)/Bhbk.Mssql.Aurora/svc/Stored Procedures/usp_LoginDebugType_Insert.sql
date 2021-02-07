
CREATE PROCEDURE [svc].[usp_LoginDebugType_Insert]
	@Id int
   ,@Name nvarchar(16)
   ,@Description nvarchar(256)
   ,@IsEnabled bit
   ,@IsEditable bit
   ,@IsDeletable bit

AS

SET NOCOUNT ON;

INSERT INTO [dbo].[tbl_LoginDebugType] 
	(Id
	,Name
    ,Description
    ,IsEnabled
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
FROM [dbo].[tbl_LoginDebugType] 
WHERE Id = @Id