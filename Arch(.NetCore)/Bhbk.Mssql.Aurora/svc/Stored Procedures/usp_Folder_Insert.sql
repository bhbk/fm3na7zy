
CREATE PROCEDURE [svc].[usp_Folder_Insert]
     @UserId				UNIQUEIDENTIFIER
    ,@ParentId				UNIQUEIDENTIFIER
    ,@VirtualName			NVARCHAR (MAX) 
    ,@IsReadOnly			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

    	BEGIN TRANSACTION;

		DECLARE @FOLDERID UNIQUEIDENTIFIER = NEWID()
        DECLARE @CREATEDUTC DATETIMEOFFSET (7) = GETUTCDATE()

		INSERT INTO [dbo].[tbl_Folder]
			(
			 Id         
			,UserId
			,ParentId
			,VirtualName  
			,IsReadOnly
			,CreatedUtc
			,LastAccessedUtc
			,LastUpdatedUtc
			)
		VALUES
			(
			 @FOLDERID          
			,@UserId
			,@ParentId
			,@VirtualName
			,@IsReadOnly
			,@CREATEDUTC
			,@CREATEDUTC
			,@CREATEDUTC
			);

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

		SELECT * FROM [dbo].[tbl_Folder] 
			WHERE Id = @FOLDERID

    	COMMIT TRANSACTION;

    END TRY

    BEGIN CATCH

    	ROLLBACK TRANSACTION;
        THROW;

    END CATCH

END
