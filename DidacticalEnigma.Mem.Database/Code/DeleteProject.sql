DROP PROCEDURE IF EXISTS "DeleteProject";

CREATE PROCEDURE "DeleteProject"
(
    "InputProjectName" character varying(32),
    "StatusCode" inout int
)
AS $$
DECLARE "DeletedCount" INTEGER;
BEGIN
    WITH deleted AS
    (DELETE FROM "Projects"
    WHERE "Name" = "InputProjectName" RETURNING *)
    SELECT COUNT(*) INTO "DeletedCount" FROM deleted;

    IF "DeletedCount" <> 0 THEN
        SELECT 0 INTO "StatusCode";
    ELSE
        SELECT 1 INTO "StatusCode";
    END IF;
END
$$
LANGUAGE plpgsql;