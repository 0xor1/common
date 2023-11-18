DROP DATABASE IF EXISTS Common;
CREATE DATABASE Common
CHARACTER SET = 'utf8mb4'
COLLATE = 'utf8mb4_general_ci';
USE Common;

DROP TABLE IF EXISTS Auths;
CREATE TABLE Auths (
    Id CHAR(22) NOT NULL,
    Email VARCHAR(250) NOT NULL,
    LastSignedInOn DATETIME(3) NOT NULL,
    LastSignInAttemptOn DATETIME(3) NOT NULL,
    ActivatedOn DATETIME(3) NOT NULL,
    NewEmail VARCHAR(250) NULL,
    VerifyEmailCodeCreatedOn DATETIME(3) NOT NULL,
    VerifyEmailCode VARCHAR(50) NOT NULL,
    ResetPwdCodeCreatedOn DATETIME(3) NOT NULL,
    ResetPwdCode VARCHAR(50) NOT NULL,
    MagicLinkCodeCreatedOn DATETIME(3) NOT NULL,
    MagicLinkCode VARCHAR(50) NOT NULL,
    Use2FA       BOOLEAN NOT NULL,
    Lang VARCHAR(7) NOT NULL,
    DateFmt TINYINT NOT NULL,
    TimeFmt VARCHAR(10) NOT NULL,
    DateSeparator VARCHAR(10) NOT NULL,
    ThousandsSeparator VARCHAR(10) NOT NULL,
    DecimalSeparator VARCHAR(10) NOT NULL,
    FcmEnabled BOOLEAN NOT NULL,
    PwdVersion INT NOT NULL,
    PwdSalt    VARBINARY(16) NOT NULL,
    PwdHash    VARBINARY(32) NOT NULL,
    PwdIters   INT NOT NULL,
    PRIMARY KEY Id (Id),
    UNIQUE INDEX Email (Email),
    UNIQUE INDEX NewEmail (NewEmail),
    INDEX(ActivatedOn, VerifyEmailCodeCreatedOn)
);

# cleanup old registrations that have not been activated in a week
SET GLOBAL event_scheduler=ON;
DROP EVENT IF EXISTS AuthRegistrationCleanup;
CREATE EVENT AuthRegistrationCleanup
ON SCHEDULE EVERY 24 HOUR
STARTS CURRENT_TIMESTAMP + INTERVAL 1 HOUR
DO DELETE FROM Auths WHERE ActivatedOn=CAST('0001-01-01 00:00:00.000' AS DATETIME(3)) AND VerifyEmailCodeCreatedOn < DATE_SUB(NOW(), INTERVAL 7 DAY);

DROP TABLE IF EXISTS FcmRegs;
CREATE TABLE FcmRegs (
     Topic VARCHAR(255) NOT NULL,
     Token VARCHAR(255) NOT NULL,
     User VARCHAR(22) NOT NULL,
     Client VARCHAR(22) NOT NULL,
     CreatedOn DATETIME(3) NOT NULL,
     FcmEnabled BOOLEAN NOT NULL,
     Primary KEY (Topic, Token),
     UNIQUE INDEX (Client),
     INDEX (User, CreatedOn),
     INDEX(Topic, FcmEnabled, Token),
     INDEX(CreatedOn)
);

# cleanup old fcm tokens that were createdOn over 2 days ago
SET GLOBAL event_scheduler=ON;
DROP EVENT IF EXISTS FcmTokenCleanup;
CREATE EVENT FcmTokenCleanup
ON SCHEDULE EVERY 24 HOUR
STARTS CURRENT_TIMESTAMP + INTERVAL 1 HOUR
DO DELETE FROM FcmRegs WHERE CreatedOn < DATE_SUB(NOW(), INTERVAL 2 DAY);


DROP USER IF EXISTS 'Common'@'%';
CREATE USER 'Common'@'%' IDENTIFIED BY 'C0-Mm-0n-C0-Mm-0n';
GRANT SELECT ON Common.* TO 'Common'@'%';
GRANT INSERT ON Common.* TO 'Common'@'%';
GRANT UPDATE ON Common.* TO 'Common'@'%';
GRANT DELETE ON Common.* TO 'Common'@'%';
GRANT EXECUTE ON Common.* TO 'Common'@'%';