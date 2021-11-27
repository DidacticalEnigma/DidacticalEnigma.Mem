DROP ROUTINE IF EXISTS "AddProject";

CREATE FUNCTION "AddProject"
(
    "InputProjectName" character varying(32)
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

    IF "ProjectId" IS NOT NULL THEN
        RETURN QUERY SELECT '{}'::jsonb AS "Result", 1 AS "StatusCode" FROM "Projects" LIMIT 1;
        RETURN;
    END IF;

    INSERT INTO "Projects" ("Name")
    VALUES ("InputProjectName");

    RETURN QUERY SELECT '{}'::jsonb AS "Result", 0 AS "StatusCode" FROM "Projects" LIMIT 1;
    RETURN;
END
$$
LANGUAGE plpgsql;