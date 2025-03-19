CREATE TABLE AspNetUsers
(
    Id NVARCHAR(450) PRIMARY KEY,
    FirstName NVARCHAR(50),
    LastName NVARCHAR(50), 
    Email NVARCHAR(50), 
    IdNumber NVARCHAR(11),
	UserName NVARCHAR(50),
    BirthDate DATETIME2
)
