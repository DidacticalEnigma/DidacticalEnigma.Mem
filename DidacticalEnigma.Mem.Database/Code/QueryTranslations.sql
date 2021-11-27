DROP ROUTINE IF EXISTS "QueryTranslations";

CREATE FUNCTION "QueryTranslations"
(
    "InputProjectName" character varying(32),
    "InputCorrelationId" character varying(256),
    "NormalizedQueryText" character varying(4096),
    "Limit" int
)
RETURNS TABLE
(
    "ParentName" character varying(32),
    "Source" character varying(4096),
    "Target" character varying(4096),
    "CorrelationId" character varying(256),
    "Context" uuid
)
STABLE
AS $$
SELECT
    P."Name" AS "ParentName",
    TP."Source" AS "Source",
    TP."Target" AS "Target",
    TP."CorrelationId" AS "CorrelationId",
    TP."ContextId" AS "Context"
FROM
    "TranslationPairs" TP
JOIN
    "Projects" P ON P."Id" = TP."ParentId"
WHERE 1=1
AND ("InputProjectName" IS NULL OR P."Name" = "InputProjectName")
AND ("InputCorrelationId" IS NULL OR starts_with(TP."CorrelationId", "InputCorrelationId"))
AND ("NormalizedQueryText" IS NULL OR phraseto_tsquery('simple', "NormalizedQueryText") @@ TP."SearchVector")
LIMIT "Limit";
$$
LANGUAGE SQL;