DROP PROCEDURE IF EXISTS "DeleteTranslation";

CREATE PROCEDURE "DeleteTranslation"
(
    "InputCorrelationId" character varying(256),
    "InputProjectName" character varying(32),
    "StatusCode" inout int
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
        SELECT 0 INTO "StatusCode";
    ELSE
        SELECT 1 INTO "StatusCode";
    END IF;
END
$$
LANGUAGE plpgsql;