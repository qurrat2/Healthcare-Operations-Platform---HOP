USE [healthcare];
GO

UPDATE dbo.users
SET password_hash = '$2a$11$tHZS1C7sXQW0ugPilmr9FeAkIoJhk8UTLKHdET8sim.Jx1qxt3Qku'
WHERE username = 'admin';
GO

UPDATE dbo.users
SET password_hash = '$2a$11$0dDGr0waPbIF7mCWUzVGt.tu7IUrIfJAPRgiTf0dmGpcLhgfVkGuG'
WHERE username = 'reception1';
GO

UPDATE dbo.users
SET password_hash = '$2a$11$QIccM9e8m7M0E9P8sO26MuvOptcN2IqLlcxjeH/6ASkTI8Nq1nBqm'
WHERE username = 'dr.asim';
GO

UPDATE dbo.users
SET password_hash = '$2a$11$MkAHnu7.9XOasPHS.d1B7uuYXkUXqYNPGohM5ELvDt422BIxrVEcC'
WHERE username = 'dr.sara';
GO
