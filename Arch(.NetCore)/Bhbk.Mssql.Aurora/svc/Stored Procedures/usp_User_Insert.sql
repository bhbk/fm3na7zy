﻿




CREATE PROCEDURE [svc].[usp_User_Insert]
     @IdentityId			UNIQUEIDENTIFIER
    ,@IdentityAlias			NVARCHAR (128) 
    ,@RequirePassword		BIT
    ,@RequirePublicKey		BIT
    ,@FileSystemType		NVARCHAR (16) 
    ,@FileSystemReadOnly	BIT
    ,@DebugLevel			NVARCHAR (16) 
    ,@IsEnabled				BIT
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        DECLARE @CREATEDUTC DATETIMEOFFSET (7) = GETUTCDATE()

		INSERT INTO [dbo].[tbl_User]
			(
			 IdentityId
			,IdentityAlias
			,RequirePassword
			,RequirePublicKey
			,FileSystemType
			,FileSystemReadOnly
			,DebugLevel
			,IsEnabled
			,IsDeletable
			,CreatedUtc
			)
		VALUES
			(
			@IdentityId
			,@IdentityAlias
			,@RequirePassword
			,@RequirePublicKey
			,@FileSystemType
			,@FileSystemReadOnly
			,@DebugLevel
			,@IsEnabled
			,@IsDeletable
			,@CREATEDUTC
			);

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

		SELECT * FROM [dbo].[tbl_User] WHERE IdentityId = @IdentityId

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END