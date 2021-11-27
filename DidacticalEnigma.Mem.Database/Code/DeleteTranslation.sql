DROP ROUTINE IF EXISTS "DeleteTranslation";

CREATE FUNCTION "DeleteTranslation"
(
    "InputCorrelationId" character varying(256),
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
    (DELETE FROM "TranslationPairs" TP
    USING "Projects" P
    WHERE TP."CorrelationId" = "InputCorrelationId"
    AND TP."ParentId" = P."Id"
    AND P."Name" = "InputProjectName"
    RETURNING *)
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