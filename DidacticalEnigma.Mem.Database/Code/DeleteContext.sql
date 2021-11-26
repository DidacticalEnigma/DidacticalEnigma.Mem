DROP PROCEDURE IF EXISTS "DeleteContext";

CREATE PROCEDURE "DeleteContext"
(
    "InputContextId" uuid,
    "StatusCode" inout int
)
AS $$
DECLARE "DeletedCount" INTEGER;
BEGIN
    WITH deleted AS
    (DELETE FROM "Contexts"
    WHERE "Id" = "InputContextId" RETURNING *)
    SELECT COUNT(*) INTO "DeletedCount" FROM deleted;

    IF "DeletedCount" <> 0 THEN
        SELECT 0 INTO "StatusCode";
    ELSE
        SELECT 1 INTO "StatusCode";
    END IF;
END
$$
LANGUAGE plpgsql;