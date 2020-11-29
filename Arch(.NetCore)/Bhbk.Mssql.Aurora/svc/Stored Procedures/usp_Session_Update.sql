
CREATE PROCEDURE [svc].[usp_Session_Update]
    @Id						    uniqueidentifier
   ,@IdentityId					uniqueidentifier
   ,@CallPath					varchar(256)
   ,@Details					varchar(MAX)
   ,@LocalEndPoint				varchar(128)
   ,@LocalSoftwareIdentifier	varchar(128)
   ,@RemoteEndPoint				varchar(128)
   ,@RemoteSoftwareIdentifier	varchar(128)

AS

SET NOCOUNT ON;

DECLARE @UPDATED DATETIMEOFFSET = GETUTCDATE()

UPDATE [dbo].[tbl_Session] 
SET  IdentityId					=	@IdentityId
    ,CallPath					=   @CallPath
    ,Details					=   @Details
    ,LocalEndPoint				=   @LocalEndPoint
    ,LocalSoftwareIdentifier    =   @LocalSoftwareIdentifier
    ,RemoteEndPoint				=   @RemoteEndPoint
    ,RemoteSoftwareIdentifier	=   @RemoteSoftwareIdentifier
WHERE Id = @Id;

    /*  Select all entity values to return
        ----------------------------------------------------
       */
	SELECT * FROM [dbo].[tbl_Session] WHERE Id = @Id
