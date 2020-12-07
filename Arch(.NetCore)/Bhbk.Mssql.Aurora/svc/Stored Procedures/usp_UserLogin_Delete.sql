
CREATE   PROCEDURE [svc].[usp_UserLogin_Delete]
    @IdentityId uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        DECLARE @Error_Message varchar(MAX);

        SELECT * FROM [dbo].[tbl_UserLogin] 
			WHERE IdentityId = @IdentityId

		IF EXISTS (SELECT 1
				   FROM [dbo].[tbl_UserLogin]
				   WHERE IdentityId = @IdentityId 
					   AND IsDeletable = 0)
			BEGIN
				SET @Error_Message = FORMATMESSAGE(
					'DELETE not allowed, Id (%s) is not deletable.'
					,CONVERT(varchar, COALESCE(@IdentityId, '')));
				THROW 50000, @Error_Message, 1;
			END;

		IF EXISTS (SELECT 1
				   FROM [dbo].[tbl_UserFile]
				   WHERE IdentityId = @IdentityId)
			BEGIN
				SET @Error_Message = FORMATMESSAGE(
					'DELETE not allowed, Id (%s) has files.'
					,CONVERT(varchar, COALESCE(@IdentityId, '')));
				THROW 50000, @Error_Message, 1;
			END;

		BEGIN TRANSACTION;

		-- Delete PublicKey

			DELETE [dbo].[tbl_PublicKey] 
			WHERE IdentityId = @IdentityId;

		-- Delete PrivateKey

			DELETE [dbo].[tbl_PrivateKey] 
			WHERE IdentityId = @IdentityId;

		-- Delete Network

			DELETE [dbo].[tbl_Network] 
			WHERE IdentityId = @IdentityId;

		-- Delete Setting

			DELETE [dbo].[tbl_Setting] 
			WHERE IdentityId = @IdentityId;

		-- Delete UserAlert

			DELETE [dbo].[tbl_UserAlert] 
			WHERE IdentityId = @IdentityId;

		-- Delete UserFolder

			DELETE [dbo].[tbl_UserFolder] 
			WHERE IdentityId = @IdentityId;

		-- Delete UserMount

			DELETE [dbo].[tbl_UserMount] 
			WHERE IdentityId = @IdentityId;

		-- Delete UserLogin

			DELETE [dbo].[tbl_UserLogin]
			WHERE IdentityId = @IdentityId

		COMMIT TRANSACTION;

    END TRY

    BEGIN CATCH
    	ROLLBACK TRANSACTION;
        THROW;

    END CATCH

END