DROP ROUTINE IF EXISTS "AddContext";

CREATE FUNCTION "AddContext"
(
    "InputContextId" uuid,
    "InputText" character varying(512),
    "InputContent" bytea,
    "InputMediaType" character varying(64)
)
RETURNS TABLE
(
    "Result" jsonb,
    "StatusCode" int
)
AS $$
DECLARE "MediaTypeId" INTEGER;
BEGIN

    SELECT "Id" INTO "MediaTypeId" FROM "MediaTypes" M WHERE M."MediaType" = "InputMediaType";

    IF "MediaTypeId" IS NULL THEN
        RETURN QUERY SELECT '{}'::jsonb AS "Result", 1 AS "StatusCode" FROM "Contexts";
        RETURN;
    END IF;

    IF EXISTS (SELECT * FROM "Contexts" WHERE "Id" = "InputContextId") THEN
        RETURN QUERY SELECT '{}'::jsonb AS "Result", 2 AS "StatusCode" FROM "Contexts";
        RETURN;
    END IF;

    INSERT INTO "Contexts" ("Id", "Text", "Content", "MediaTypeId")
    VALUES ("InputContextId", "InputText", "InputContent", "MediaTypeId");

    RETURN QUERY SELECT '{}'::jsonb AS "Result", 0 AS "StatusCode" FROM "Contexts";
    RETURN;

END
$$
LANGUAGE plpgsql;