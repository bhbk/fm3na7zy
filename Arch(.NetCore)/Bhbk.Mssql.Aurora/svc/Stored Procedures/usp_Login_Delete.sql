
CREATE   PROCEDURE [svc].[usp_Login_Delete]
    @UserId uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        DECLARE @Error_Message varchar(MAX);

        SELECT * FROM [dbo].[tbl_Login] 
			WHERE UserId = @UserId

		IF EXISTS (SELECT 1
				   FROM [dbo].[tbl_Login]
				   WHERE UserId = @UserId 
					   AND IsDeletable = 0)
			BEGIN
				SET @Error_Message = FORMATMESSAGE(
					'DELETE not allowed, Id (%s) is not deletable.'
					,CONVERT(varchar, COALESCE(@UserId, '')));
				THROW 50000, @Error_Message, 1;
			END;

		IF EXISTS (SELECT 1
				   FROM [dbo].[tbl_File]
				   WHERE UserId = @UserId)
			BEGIN
				SET @Error_Message = FORMATMESSAGE(
					'DELETE not allowed, Id (%s) has files.'
					,CONVERT(varchar, COALESCE(@UserId, '')));
				THROW 50000, @Error_Message, 1;
			END;

		BEGIN TRANSACTION;

		-- Delete PrivateKey

			DELETE [dbo].[tbl_PrivateKey] 
			WHERE UserId = @UserId;

		-- Delete PublicKey

			DELETE [dbo].[tbl_PublicKey] 
			WHERE UserId = @UserId;

		-- Delete Network

			DELETE [dbo].[tbl_Network] 
			WHERE UserId = @UserId;

		-- Delete Setting

			DELETE [dbo].[tbl_Setting] 
			WHERE UserId = @UserId;

		-- Delete UserUsage

			DELETE [dbo].[tbl_Usage] 
			WHERE UserId = @UserId;

		-- Delete Alert

			DELETE [dbo].[tbl_Alert] 
			WHERE UserId = @UserId;

		-- Delete Folder

			DELETE [dbo].[tbl_Folder] 
			WHERE UserId = @UserId;

		-- Delete Mount

			DELETE [dbo].[tbl_Mount] 
			WHERE UserId = @UserId;

		-- Delete Login

			DELETE [dbo].[tbl_Login]
			WHERE UserId = @UserId

		COMMIT TRANSACTION;

    END TRY

    BEGIN CATCH
    	ROLLBACK TRANSACTION;
        THROW;

    END CATCH

END