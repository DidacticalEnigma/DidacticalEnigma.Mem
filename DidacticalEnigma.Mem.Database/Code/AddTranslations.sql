DROP PROCEDURE IF EXISTS "AddTranslations";

CREATE PROCEDURE "AddTranslations"
(
    "InputProjectName" character varying(32),
    "CurrentTime" timestamp without time zone,
    "AllowPartialAdd" bool,
    "Translations" jsonb,
    "Result" inout jsonb,
    "StatusCode" inout int
)
AS $$
DECLARE "ProjectId" INTEGER;
BEGIN
    SELECT '{"NotAdded":[]}'::jsonb INTO "Result";

    SELECT "Id" INTO "ProjectId" FROM "Projects" P WHERE P."Name" = "InputProjectName";

    IF "ProjectId" IS NULL THEN
        SELECT 1 INTO "StatusCode";
        RETURN;
    END IF;

    CREATE TEMPORARY TABLE "InputTranslations" (
        "Id" uuid,
        "NormalizedSource" character varying(8192),
        "Source" character varying(4096),
        "Target" character varying(4096),
        "CorrelationId" character varying(256),
        "Context" uuid);

    INSERT INTO "InputTranslations"
    SELECT * FROM jsonb_to_recordset("Translations") AS x(
        "Id" uuid,
        "NormalizedSource" character varying(8192),
        "Source" character varying(4096),
        "Target" character varying(4096),
        "CorrelationId" character varying(256),
        "Context" uuid);

    IF (SELECT COUNT("CorrelationId") <> COUNT(DISTINCT "CorrelationId") FROM "InputTranslations") THEN
        SELECT 2 INTO "StatusCode";
        RETURN;
    END IF;

    IF (SELECT COUNT(*) <> 0 FROM "Contexts" WHERE "Id" NOT IN (SELECT "Context" FROM "InputTranslations")) THEN
        SELECT 3 INTO "StatusCode";
        RETURN;
    END IF;

    IF "AllowPartialAdd" AND (SELECT COUNT(*) <> 0 FROM "InputTranslations" WHERE "CorrelationId" IN (SELECT "CorrelationId" FROM "TranslationPairs")) THEN
        SELECT 4 INTO "StatusCode";
        RETURN;
    END IF;

    INSERT INTO "TranslationPairs" ("Id", "CorrelationId", "Source", "SearchVector", "Target", "ContextId", "ParentId", "CreationTime", "ModificationTime")
    SELECT "Id", "CorrelationId", "Source", to_tsvector('simple', "NormalizedSource"), "Target", "Context", "ProjectId", "CurrentTime", "CurrentTime"
    FROM "InputTranslations" WHERE "CorrelationId" NOT IN (SELECT "CorrelationId" FROM "TranslationPairs");

    SELECT 0 INTO "StatusCode";
END
$$
LANGUAGE plpgsql;