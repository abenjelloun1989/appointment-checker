# appointment-checker
C# Console app that checks for appointments 

1 - enable the appointment you would like to check in /script/startup.sh
2 - run : docker compose build --no-cache
3 - run : docker run appointmentchecker:latest

you can configure a cron to run it every x minutes : 
crond -e
*/1 * * * * docker run -d appointmentchecker:latest > /dev/null
sudo service cron start
