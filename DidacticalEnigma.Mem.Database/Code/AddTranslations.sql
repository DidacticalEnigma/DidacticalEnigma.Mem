DROP ROUTINE IF EXISTS "AddTranslations";

CREATE FUNCTION "AddTranslations"
(
    "InputProjectName" character varying(32),
    "CurrentTime" timestamp without time zone,
    "AllowPartialAdd" bool,
    "Translations" jsonb
)
RETURNS TABLE
(
    "Result" jsonb,
    "StatusCode" int
)
AS $$
DECLARE "ProjectId" INTEGER;
BEGIN
    SELECT "Id" INTO "ProjectId" FROM "Projects" P WHERE P."Name" = "InputProjectName";

    IF "ProjectId" IS NULL THEN
        RETURN QUERY SELECT '{"NotAdded":[]}'::jsonb AS "Result", 1 AS "StatusCode" FROM "TranslationPairs";
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
        RETURN QUERY SELECT '{"NotAdded":[]}'::jsonb AS "Result", 2 AS "StatusCode" FROM "TranslationPairs";
        RETURN;
    END IF;

    IF (SELECT COUNT(*) <> 0 FROM "Contexts" WHERE "Id" NOT IN (SELECT "Context" FROM "InputTranslations")) THEN
        RETURN QUERY SELECT '{"NotAdded":[]}'::jsonb AS "Result", 3 AS "StatusCode" FROM "TranslationPairs";
        RETURN;
    END IF;

    IF "AllowPartialAdd" AND (SELECT COUNT(*) <> 0 FROM "InputTranslations" WHERE "CorrelationId" IN (SELECT "CorrelationId" FROM "TranslationPairs")) THEN
        RETURN QUERY SELECT '{"NotAdded":[]}'::jsonb AS "Result", 4 AS "StatusCode" FROM "TranslationPairs";
        RETURN;
    END IF;

    INSERT INTO "TranslationPairs" ("Id", "CorrelationId", "Source", "SearchVector", "Target", "ContextId", "ParentId", "CreationTime", "ModificationTime")
    SELECT "Id", "CorrelationId", "Source", to_tsvector('simple', "NormalizedSource"), "Target", "Context", "ProjectId", "CurrentTime", "CurrentTime"
    FROM "InputTranslations" WHERE "CorrelationId" NOT IN (SELECT "CorrelationId" FROM "TranslationPairs");

    -- TODO: return all the entries that were not added in the JSON
    RETURN QUERY SELECT '{"NotAdded":[]}'::jsonb AS "Result", 0 AS "StatusCode" FROM "TranslationPairs";
    RETURN;
END
$$
LANGUAGE plpgsql;