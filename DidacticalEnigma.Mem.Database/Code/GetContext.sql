DROP PROCEDURE IF EXISTS "GetContext";

CREATE PROCEDURE "GetContext"
(
    "InputContextId" uuid,
    "Result" inout jsonb,
    "StatusCode" inout int
)
AS $$
DECLARE "MediaTypeId" INTEGER;
BEGIN

    IF NOT EXISTS (SELECT * FROM "Contexts" WHERE "Id" = "InputContextId") THEN
        SELECT 1 INTO "StatusCode";
        RETURN;
    END IF;

    SELECT json_agg(row_to_json(data)) INTO "Result"
    FROM (
        SELECT "Contexts"."Content" AS "Content", "MediaTypes"."MediaType" AS "MediaType", "Contexts"."Text" AS "Text"
        FROM "Contexts"
        JOIN "MediaTypes" on "MediaTypes"."Id" = "Contexts"."MediaTypeId"
        WHERE "Contexts"."Id" = "InputContextId") data;

    SELECT 0 INTO "StatusCode";

END
$$
LANGUAGE plpgsql;