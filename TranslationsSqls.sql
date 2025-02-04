
/* See EMSuite entries */
/*select * from Translation where SystemEnum = 1;*/


/* See Valsuite entries */
/*select * from Translation where SystemEnum = 0;*/

/* Delete Duplicates */
/*
WITH added_row_number AS (
  SELECT
    *,
    ROW_NUMBER() OVER(PARTITION BY "Key" + LanguageCode + ResourceName ORDER BY ID ASC) AS row_number
  FROM Translation
)
SELECT
  *
FROM added_row_number
WHERE row_number <> 1;*/