PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS Problem_Category (
                Problem_Category_id INTEGER PRIMARY KEY,
                Problem_Category_Name TEXT NOT NULL CHECK(length(Problem_Category_Name) <= 50)
) ; ---------------------------------------------------------------------------------------------


CREATE TABLE IF NOT EXISTS City (
                City_id INTEGER PRIMARY KEY,
                City_Name TEXT NOT NULL CHECK(length(City_Name) <= 50),
                State_Abbreviation TEXT NOT NULL CHECK(length(State_Abbreviation) <= 10)
) ; ---------------------------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS District (
                District_id INTEGER PRIMARY KEY,
                District_Name TEXT NOT NULL CHECK(length(District_Name) <= 50),
                City_id INTEGER NOT NULL,
				FOREIGN KEY (City_id) REFERENCES City(City_id)
) ; ---------------------------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS Base_Account (
                Base_Account_id INTEGER PRIMARY KEY,
                Username TEXT NOT NULL CHECK(length(Username) <= 50),
                Hashed_password TEXT NOT NULL,
                Email TEXT NOT NULL CHECK(length(Email) <= 100),
                Description TEXT CHECK(length(Description) <= 500),
                Profile_picture_link TEXT,
                UTC_datetime_creation INTEGER DEFAULT( unixepoch('now') ) NOT NULL, --UTC (in seconds)
                Advertisement_slots_amount INTEGER DEFAULT 0 NOT NULL,
                District_id INTEGER NOT NULL,
                FOREIGN KEY(District_id) REFERENCES District(District_id)
) ; ---------------------------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS Person_Account (
                Person_Account_id INTEGER PRIMARY KEY NOT NULL,
                Birthday TEXT NOT NULL CHECK( Birthday IS date(Birthday) ), -- 'YYYY-MM-DD' (ISO 8601)
                Informal_Work TEXT CHECK( length(Informal_Work) <= 50 ),
                FOREIGN KEY (Person_Account_id) REFERENCES Base_Account(Base_Account_id)
) ; ---------------------------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS Business_Account(
	Business_Account_id INTEGER PRIMARY KEY NOT NULL,
	Cnpj TEXT NOT NULL,
	FOREIGN KEY (Business_Account_id) REFERENCES Base_Account(Base_Account_id)
); ---------------------------------------------------------------------------------------------


CREATE TABLE IF NOT EXISTS Report (
                Report_id INTEGER PRIMARY KEY,
                UTC_Date_Report INTEGER DEFAULT( unixepoch('now') ) NOT NULL, --UTC (in seconds)
				
                Is_Fixed BOOLEAN DEFAULT(FALSE) NOT NULL, 

                Problem_Category_id INTEGER NOT NULL,
                Reported_District_id INTEGER NOT NULL,
                Base_Account_id INTEGER, -- an account is not required to make a report
                
				FOREIGN KEY(Problem_Category_id) REFERENCES Problem_Category(Problem_Category_id),
				FOREIGN KEY(Reported_District_id) REFERENCES District(District_id),
				FOREIGN KEY(Base_Account_id) REFERENCES Base_Account(Base_Account_id)
) ; ---------------------------------------------------------------------------------------------


CREATE TABLE IF NOT EXISTS Recent_Report( --Aux table to index specific (recent) reports
                                          --Most Statistics are pulled only from the reports of this table
	Report_id INTEGER PRIMARY KEY NOT NULL,
	FOREIGN KEY (Report_id) REFERENCES Report(Report_id)
); ---------------------------------------------------------------------------------------------


CREATE TABLE IF NOT EXISTS Message  (
	Message_id INTEGER PRIMARY KEY,
	
	Message_text TEXT NOT NULL CHECK(length(Message_text) <= 1000),
	Message_image_link TEXT,
	UTC_datetime_sent INTEGER DEFAULT ( unixepoch('now') ) NOT NULL ,
	Is_hidden BOOLEAN DEFAULT (FALSE) NOT NULL,
	
	Base_Account_id INTEGER NOT NULL,
	
	FOREIGN KEY (Base_Account_id) REFERENCES Base_Account(Base_Account_id)
); ---------------------------------------------------------------------------------------------


CREATE TABLE IF NOT EXISTS Chat  (
	Chat_id INTEGER PRIMARY KEY,
	District_id INTEGER NOT NULL,

	FOREIGN KEY (District_id) REFERENCES District(District_id)
); ---------------------------------------------------------------------------------------------


CREATE TABLE IF NOT EXISTS Chat_has_message  (
	Chat_has_message_id INTEGER PRIMARY KEY,
	
	Message_id INTEGER NOT NULL,
	Chat_id INTEGER NOT NULL,

	FOREIGN KEY (Message_id) REFERENCES Message(Message_id),
	FOREIGN KEY (Chat_id) REFERENCES Chat(Chat_id)
); ---------------------------------------------------------------------------------------------