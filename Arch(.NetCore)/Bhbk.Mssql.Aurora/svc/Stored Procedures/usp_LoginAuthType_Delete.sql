
CREATE PROCEDURE [svc].[usp_LoginAuthType_Delete]
	@Id int

AS

SET NOCOUNT ON;

DECLARE @Error_Message varchar(MAX);

IF EXISTS (SELECT 1
		   FROM [dbo].[tbl_LoginAuthType]
		   WHERE Id = @Id 
			   AND IsDeletable = 0)     
	BEGIN
		SET @Error_Message = FORMATMESSAGE(
			'DELETE not allowed, Id (%s) is not deletable.'
			,CONVERT(varchar, COALESCE(@Id, '')));
		THROW 50000, @Error_Message, 1;
	END;

DELETE FROM [dbo].[tbl_LoginAuthType]
WHERE Id = @Id;