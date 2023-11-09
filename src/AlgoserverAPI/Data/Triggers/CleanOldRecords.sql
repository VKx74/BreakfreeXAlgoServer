DROP EVENT IF EXISTS CleanOldRecords;

DROP EVENT IF EXISTS CleanOldRecordsOnNALogs;
CREATE EVENT CleanOldRecordsOnNALogs
ON SCHEDULE EVERY 1 DAY
DO
BEGIN
 DELETE FROM algostatistics.NALogs
 WHERE (`Date` < date_sub(curdate(), INTERVAL 5 DAY) AND Id != '');
END