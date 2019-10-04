
import mysql.connector;
from datetime import date

db = mysql.connector.connect(
	host="175.207.238.104",
	port=3700,
	user="power21",
	passwd="123qwe",
	database="etri")

cursor = db.cursor()

sql = """SELECT * FROM etri.tb_command where now() >= date order by date desc limit 1""";

cursor.execute(sql)
result = cursor.fetchone()

print(result)

print(result[0])