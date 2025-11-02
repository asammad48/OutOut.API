cd /root/outout

docker compose down


# Remove existing handire from linux docker
docker exec -it outout-mongo mongosh

use Hangfire_Dev

db.dropDatabase()

exit
#end

ls /root/mongo_backup

docker cp /root/mongo_backup outout-mongo:/backup

docker exec -it outout-mongo mongoimport --db OutOut_Dev --collection Users --file /backup/OutOut_Dev.Users.json --jsonArray
docker exec -it outout-mongo mongoimport --db OutOut_Dev --collection Events --file /backup/OutOut_Dev.Events.json --jsonArray
docker exec -it outout-mongo mongoimport --db OutOut_Dev --collection Categories --file /backup/OutOut_Dev.Categories.json --jsonArray
docker exec -it outout-mongo mongoimport --db OutOut_Dev --collection Cities --file /backup/OutOut_Dev.Cities.json --jsonArray
docker exec -it outout-mongo mongoimport --db OutOut_Dev --collection Roles --file /backup/OutOut_Dev.Roles.json --jsonArray
docker exec -it outout-mongo mongoimport --db OutOut_Dev --collection LoyaltyType --file /backup/OutOut_Dev.LoyaltyType.json --jsonArray

docker exec -it outout-mongo mongosh

use OutOut_Dev
show collections
db.Users.countDocuments()
db.Users.find().pretty()


docker compose up -d --build

docker ps

docker logs outout-api


Using MongoDB database: OutOut_Dev


http://165.22.210.43:8080/swagger
