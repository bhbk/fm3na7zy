
CREATE PROCEDURE [svc].[usp_Setting_Update]
     @Id					UNIQUEIDENTIFIER 
    ,@IdentityId			UNIQUEIDENTIFIER
    ,@ConfigKey				NVARCHAR (MAX) 
    ,@ConfigValue			NVARCHAR (MAX) 
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        UPDATE [dbo].[tbl_Setting]
        SET
	         ConfigKey				= @ConfigKey
	        ,ConfigValue			= @ConfigValue
            ,IsDeletable			= @IsDeletable
        WHERE Id = @Id AND IdentityId = @IdentityId

        SELECT * FROM [dbo].[tbl_Setting] 
			WHERE Id = @Id AND IdentityId = @IdentityId

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END