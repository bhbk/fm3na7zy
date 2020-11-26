






CREATE PROCEDURE [svc].[usp_UserFolder_Insert]
     @IdentityId			UNIQUEIDENTIFIER
    ,@ParentId				UNIQUEIDENTIFIER
    ,@VirtualName			NVARCHAR (MAX) 
    ,@IsReadOnly			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

		DECLARE @FOLDERID UNIQUEIDENTIFIER = NEWID()
        DECLARE @CREATEDUTC DATETIMEOFFSET (7) = GETUTCDATE()

		INSERT INTO [dbo].[tbl_UserFolder]
			(
			 Id         
			,IdentityId
			,ParentId
			,VirtualName  
			,IsReadOnly
			,CreatedUtc
			)
		VALUES
			(
			 @FOLDERID          
			,@IdentityId
			,@ParentId
			,@VirtualName
			,@IsReadOnly
			,@CREATEDUTC
			);

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

		SELECT * FROM [dbo].[tbl_UserFolder] WHERE Id = @FOLDERID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END