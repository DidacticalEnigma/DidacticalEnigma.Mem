DROP ROUTINE IF EXISTS "DeleteContext";

CREATE FUNCTION "DeleteContext"
(
    "InputContextId" uuid
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
    (DELETE FROM "Contexts"
    WHERE "Id" = "InputContextId" RETURNING *)
    SELECT COUNT(*) INTO "DeletedCount" FROM deleted;

    IF "DeletedCount" <> 0 THEN
        RETURN QUERY SELECT '{}'::jsonb AS "Result", 0 AS "StatusCode" FROM "Contexts";
        RETURN;
    ELSE
        RETURN QUERY SELECT '{}'::jsonb AS "Result", 1 AS "StatusCode" FROM "Contexts";
        RETURN;
    END IF;
END
$$
LANGUAGE plpgsql;