DROP EVENT IF EXISTS CleanOldRecords;
CREATE EVENT CleanOldRecords
ON SCHEDULE EVERY 1 DAY
DO
BEGIN
 DELETE FROM algostatistics.Statistics
 WHERE (CreatedAt < date_sub(curdate(), INTERVAL 7 DAY) AND Id <> 0);
END