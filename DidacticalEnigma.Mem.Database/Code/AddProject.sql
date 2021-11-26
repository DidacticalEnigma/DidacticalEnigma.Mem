DROP PROCEDURE IF EXISTS "AddProject";

CREATE PROCEDURE "AddProject"
(
    "InputProjectName" character varying(32),
    "StatusCode" inout int
)
AS $$
DECLARE "ProjectId" INTEGER;
BEGIN

    SELECT "Id" INTO "ProjectId" FROM "Projects" P WHERE P."Name" = "InputProjectName";

    IF "ProjectId" IS NULL THEN
        SELECT 1 INTO "StatusCode";
        RETURN;
    END IF;

    INSERT INTO "Projects" ("Name")
    VALUES ("InputProjectName");

    SELECT 0 INTO "StatusCode";

END
$$
LANGUAGE plpgsql;