
CREATE PROCEDURE [svc].[usp_Network_Update]
     @Id					UNIQUEIDENTIFIER 
    ,@IdentityId			UNIQUEIDENTIFIER 
    ,@Address				NVARCHAR (128) 
    ,@Action				NVARCHAR (8) 
    ,@IsEnabled				BIT
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        DECLARE @LASTUPDATED DATETIMEOFFSET (7) = GETUTCDATE()

        UPDATE [dbo].[tbl_Network]
        SET
			 Address				= @Address
			,Action					= @Action
			,IsEnabled				= @IsEnabled
            ,IsDeletable            = @IsDeletable
            ,LastUpdatedUtc			= @LASTUPDATED
        WHERE Id = @Id AND IdentityId = @IdentityId

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_Network] 
			WHERE Id = @Id AND IdentityId = @IdentityId

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END