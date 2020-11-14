
CREATE VIEW [svc].[uvw_Activity]
AS
SELECT        Id, ActorId, IdentityId, ActivityType, TableName, KeyValues, OriginalValues, IsDeletable, CurrentValues, CreatedUtc
FROM            [dbo].[tbl_Activity]