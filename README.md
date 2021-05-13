# appointment-checker
C# Console app that checks for appointments 

three modes : candilib, wedding and vaccin

1 - enable the appointment you would like to check in /script/startup.sh <br />
2 - modify the email sender and receiver in app.config. For the sender, you will need to use a gmail account with the setting "Less secure apps" enabled : https://support.google.com/accounts/answer/6010255?hl=en
3 - run : docker compose build --no-cache <br />
4 - run : docker run appointmentchecker:latest <br />

you can configure a cron to run it every x minutes : <br />
crond -e <br />
*/1 * * * * docker run -d appointmentchecker:latest > /dev/null <br />
sudo service cron start <br />

you can also just run the app with a dotnet command : dotnet run [param]
param in: [candilib, weeding, vaccin]

For email sender, you will need to use a gmail account with the setting "Less secure apps" enabled : https://support.google.com/accounts/answer/6010255?hl=en
