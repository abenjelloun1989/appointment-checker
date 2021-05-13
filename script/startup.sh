dotnet appointment-checker.dll candilib
#dotnet appointment-checker.dll wedding
dotnet appointment-checker.dll vaccin

#crond -e
#*/1 * * * * docker run -d appointmentchecker:latest > /dev/null
#sudo service cron start