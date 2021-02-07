
CREATE PROCEDURE [svc].[usp_SmbAuthType_Update]
	@Id int
   ,@Name nvarchar(32)
   ,@Description nvarchar(256)
   ,@IsEnabled bit
   ,@IsEditable bit
   ,@IsDeletable bit

AS

SET NOCOUNT ON;

DECLARE @Error_Message varchar(MAX);

IF EXISTS (SELECT 1
		   FROM [dbo].[tbl_SmbAuthType]
		   WHERE Id = @Id 
			   AND IsEditable = 0)     
	BEGIN
		SET @Error_Message = FORMATMESSAGE(
			'EDIT not allowed, Id (%s) is not editable.'
			,CONVERT(varchar, COALESCE(@Id, '')));
		THROW 50000, @Error_Message, 1;
	END;

UPDATE [dbo].[tbl_SmbAuthType] 
SET Name = @Name
   ,Description = @Description
   ,IsEnabled = @IsEnabled
   ,IsEditable = @IsEditable
   ,IsDeletable = @IsDeletable
WHERE Id = @Id;

    /*  Select all entity values to return
        ----------------------------------------------------
       */
	
SELECT * 
FROM [dbo].[tbl_SmbAuthType] 
WHERE Id = @Id