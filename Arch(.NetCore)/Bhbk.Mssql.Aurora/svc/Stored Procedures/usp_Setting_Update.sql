

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
             Id						= @Id
	        ,IdentityId				= @IdentityId
	        ,ConfigKey				= @ConfigKey
	        ,ConfigValue			= @ConfigValue
            ,IsDeletable			= @IsDeletable
        WHERE Id = @Id

        SELECT * FROM [svc].[uvw_Setting] WHERE [svc].[uvw_Setting].Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END