DROP ROUTINE IF EXISTS "GetContext";

CREATE FUNCTION "GetContext"
(
    "InputContextId" uuid
)
RETURNS TABLE
(
    "Result" jsonb,
    "StatusCode" int
)
AS $$
BEGIN

    IF NOT EXISTS (SELECT * FROM "Contexts" WHERE "Id" = "InputContextId") THEN
        RETURN QUERY SELECT '{}'::jsonb AS "Result", 1 AS "StatusCode" FROM "Contexts";
        RETURN;
    END IF;

    RETURN QUERY SELECT to_jsonb(data) AS "Result", 0 AS "StatusCode"
    FROM (
        SELECT encode("Contexts"."Content", 'base64') AS "Content", "MediaTypes"."MediaType" AS "MediaType", "Contexts"."Text" AS "Text"
        FROM "Contexts"
        JOIN "MediaTypes" on "MediaTypes"."Id" = "Contexts"."MediaTypeId"
        WHERE "Contexts"."Id" = "InputContextId") data;

END
$$
LANGUAGE plpgsql;