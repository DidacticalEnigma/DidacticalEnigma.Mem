DROP PROCEDURE IF EXISTS "UpdateTranslation";

CREATE PROCEDURE "UpdateTranslation"
(
    "InputProjectName" character varying(32),
    "InputCorrelationId" character varying(32),
    "CurrentTime" timestamp without time zone,
    "InputNormalizedSource" character varying(8192),
    "InputSource" character varying(4096),
    "InputTarget" character varying(4096),
    "InputContextId" uuid,
    "StatusCode" inout int
)
AS $$
DECLARE "UsedCurrentTime" timestamp without time zone;
DECLARE "ContextId" uuid;
BEGIN
    IF "InputContextId" IS NOT NULL AND EXISTS (SELECT * FROM "Contexts" WHERE "Id" = "InputContextId") THEN
        SELECT 1 INTO "StatusCode";
        RETURN;
    END IF;

    IF NOT EXISTS (SELECT * FROM "TranslationPairs" JOIN "Projects" P on P."Id" = "TranslationPairs"."ParentId" WHERE "CorrelationId" = "InputCorrelationId" AND P."Name" = "InputProjectName") THEN
        SELECT 2 INTO "StatusCode";
        RETURN;
    END IF;

    IF "InputSource" IS NOT NULL THEN
        UPDATE "TranslationPairs"
        SET
            "SearchVector" = to_tsvector('simple', "InputNormalizedSource"),
            "Source" = "InputSource"
        WHERE "CorrelationId" = "InputCorrelationId"
        AND "ParentId" = (SELECT "Id" FROM "Projects" WHERE "Name" = "InputProjectName");

        SELECT "CurrentTime" INTO "UsedCurrentTime";
    END IF;

    IF "InputTarget" IS NOT NULL THEN
        UPDATE "TranslationPairs"
        SET
            "Target" = "InputTarget"
        WHERE "CorrelationId" = "InputCorrelationId"
        AND "ParentId" = (SELECT "Id" FROM "Projects" WHERE "Name" = "InputProjectName");

        SELECT "CurrentTime" INTO "UsedCurrentTime";
    END IF;

    IF "InputContextId" IS NOT NULL THEN
        UPDATE "TranslationPairs"
        SET
            "ContextId" = "InputContextId"
        WHERE "CorrelationId" = "InputCorrelationId"
        AND "ParentId" = (SELECT "Id" FROM "Projects" WHERE "Name" = "InputProjectName");

        SELECT "CurrentTime" INTO "UsedCurrentTime";
    END IF;

    IF "UsedCurrentTime" IS NOT NULL THEN
        UPDATE "TranslationPairs"
        SET
            "ModificationTime" = "UsedCurrentTime"
        WHERE "CorrelationId" = "InputCorrelationId"
        AND "ParentId" = (SELECT "Id" FROM "Projects" WHERE "Name" = "InputProjectName");
    END IF;

    SELECT 0 INTO "StatusCode";
END
$$
LANGUAGE plpgsql;