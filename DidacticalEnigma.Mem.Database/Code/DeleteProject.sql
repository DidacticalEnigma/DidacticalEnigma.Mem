DROP ROUTINE IF EXISTS "DeleteProject";

CREATE FUNCTION "DeleteProject"
(
    "InputProjectName" character varying(32)
)
RETURNS TABLE
(
    "Result" jsonb,
    "StatusCode" int
)
AS $$
DECLARE "DeletedCount" INTEGER;
BEGIN
    WITH deleted AS
    (DELETE FROM "Projects"
    WHERE "Name" = "InputProjectName" RETURNING *)
    SELECT COUNT(*) INTO "DeletedCount" FROM deleted;

    IF "DeletedCount" <> 0 THEN
        RETURN QUERY SELECT '{}'::jsonb AS "Result", 0 AS "StatusCode" FROM "TranslationPairs";
        RETURN;
    ELSE
        RETURN QUERY SELECT '{}'::jsonb AS "Result", 1 AS "StatusCode" FROM "TranslationPairs";
        RETURN;
    END IF;
END
$$
LANGUAGE plpgsql;