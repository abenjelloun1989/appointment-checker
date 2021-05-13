# appointment-checker
C# Console app that checks for appointments 

1 - enable the appointment you would like to check in /script/startup.sh <br />
2 - run : docker compose build --no-cache <br />
3 - run : docker run appointmentchecker:latest <br />

you can configure a cron to run it every x minutes : <br />
crond -e <br />
*/1 * * * * docker run -d appointmentchecker:latest > /dev/null <br />
sudo service cron start <br />
