
CREATE PROCEDURE [svc].[usp_PublicKeySignatureType_Insert]
	@Id int
   ,@Name nvarchar(16)
   ,@Description nvarchar(256)
   ,@IsEnabled bit
   ,@IsEditable bit
   ,@IsDeletable bit

AS

SET NOCOUNT ON;

INSERT INTO [dbo].[tbl_PublicKeySignatureType] 
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
FROM [dbo].[tbl_PublicKeySignatureType] 
WHERE Id = @Id