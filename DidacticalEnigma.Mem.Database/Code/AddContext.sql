DROP PROCEDURE IF EXISTS "AddContext";

CREATE PROCEDURE "AddContext"
(
    "InputContextId" uuid,
    "InputText" character varying(512),
    "InputContent" bytea,
    "InputMediaType" character varying(64),
    "StatusCode" inout int
)
AS $$
DECLARE "MediaTypeId" INTEGER;
BEGIN

    SELECT "Id" INTO "MediaTypeId" FROM "MediaTypes" M WHERE M."MediaType" = "InputMediaType";

    IF "MediaTypeId" IS NULL THEN
        SELECT 1 INTO "StatusCode";
        RETURN;
    END IF;

    IF EXISTS (SELECT * FROM "Contexts" WHERE "Id" = "InputContextId") THEN
        SELECT 2 INTO "StatusCode";
        RETURN;
    END IF;

    INSERT INTO "Contexts" ("Id", "Text", "Content", "MediaTypeId")
    VALUES ("InputContextId", "InputText", "InputContent", "MediaTypeId");

    SELECT 0 INTO "StatusCode";

END
$$
LANGUAGE plpgsql;