




CREATE PROCEDURE [svc].[usp_UserFolder_Update]
	 @Id					UNIQUEIDENTIFIER
    ,@IdentityId			UNIQUEIDENTIFIER
    ,@ParentId				UNIQUEIDENTIFIER
    ,@VirtualName			NVARCHAR (MAX) 
    ,@IsReadOnly			BIT
	,@LastAccessedUtc		DATETIMEOFFSET (7)
	,@LastUpdatedUtc		DATETIMEOFFSET (7)

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        DECLARE @LASTUPDATED DATETIMEOFFSET (7) = GETUTCDATE()

        UPDATE [dbo].[tbl_UserFolder]
        SET
			 VirtualName			= @VirtualName
			,IsReadOnly				= @IsReadOnly
			,LastAccessedUtc		= @LastAccessedUtc
            ,LastUpdatedUtc			= @LastUpdatedUtc
        WHERE Id = @Id AND ParentId = @ParentId AND IdentityId = @IdentityId

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_UserFolder] 
			WHERE Id = @Id AND ParentId = @ParentId AND IdentityId = @IdentityId

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END