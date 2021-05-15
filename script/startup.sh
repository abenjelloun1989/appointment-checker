#dotnet appointment-checker.dll candilib
#dotnet appointment-checker.dll wedding
dotnet appointment-checker.dll vaccin "LOCATION" "RECEIVER@RECEIVER.com"

#crond -e
#*/1 * * * * docker run -d appointmentchecker:latest > /dev/null
#*/1    07-19        *     * *     docker run -d appointmentchecker:latest > /dev/null
#sudo service cron start