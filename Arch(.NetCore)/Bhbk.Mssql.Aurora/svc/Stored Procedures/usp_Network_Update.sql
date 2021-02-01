
CREATE PROCEDURE [svc].[usp_Network_Update]
     @Id					UNIQUEIDENTIFIER 
	,@SequenceId			INT
    ,@Address				NVARCHAR (128) 
    ,@Action				NVARCHAR (8) 
    ,@IsEnabled				BIT
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

    	BEGIN TRANSACTION;

        DECLARE @LASTUPDATED DATETIMEOFFSET (7) = GETUTCDATE()

        UPDATE [dbo].[tbl_Network]
        SET
			 SequenceId				= @SequenceId
			,Address				= @Address
			,Action					= @Action
			,IsEnabled				= @IsEnabled
            ,IsDeletable            = @IsDeletable
            ,LastUpdatedUtc			= @LASTUPDATED
        WHERE Id = @Id

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_Network] 
			WHERE Id = @Id

    	COMMIT TRANSACTION;

    END TRY

    BEGIN CATCH

    	ROLLBACK TRANSACTION;
        THROW;

    END CATCH

END