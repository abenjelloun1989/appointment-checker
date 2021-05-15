# appointment-checker
C# Console app that checks for appointments 

three modes : candilib, wedding and vaccin

1 - enable the appointment you would like to check in /script/startup.sh <br />
2 - modify the email sender and receiver in app.config. For the sender, you will need to use a gmail account with the setting "Less secure apps" enabled : https://support.google.com/accounts/answer/6010255?hl=en <br />
3 - run : docker compose build --no-cache <br />
4 - run : docker compose up <br />
Or run : docker run appointmentchecker:latest <br />

you can configure a cron to run it every x minutes (ref : https://crontab.guru/) : <br />
crond -e <br />
*/1 * * * * docker run -d appointmentchecker:latest > /dev/null <br />
sudo service cron start <br />


you can also just run the app with a dotnet command : dotnet run [param] <br />
param in: [candilib, weeding, vaccin]

For email sender, you will need to use a gmail account with the setting "Less secure apps" enabled : https://support.google.com/accounts/answer/6010255?hl=en

BONUS STEP

How to deploy your app on a Raspberry PI: <br/>
1 - download and install a compatible ubuntu distrib on your SD card that you'll use on your Rpi. I personally chose Ubuntu 20.04.2 LTS on my Rpi 4 model B. <br/>
2 - from you local machine, which could be a WSL, generate a ssh private/pub keys and copy the public one in your Rpi ~/.ssh/authorized_keys file in order to be able to connect to it without any password. ref: https://www.raspberrypi.org/documentation/remote-access/ssh/passwordless.md <br/>
3 - install docker on your Rpi. <br/>
4 - create a docker group on your Rpi and add your current user to that group (you'll need to logout login in order for this to be taken into account) : <br />
    sudo groupadd docker <br />
    sudo usermod -aG docker ${USER} <br/>
4 - from your local machine, declare your Rpi as a docker remote context using the following command :  <br/>
    docker context create remote --docker "host=ssh://<USER>@<IP>" <br/>
5 - run : "docker context ps" and make sure that the remote context is created <br/>
6 - in order to set your Rpi as the default context for all your docker command just run : docker context use remote <br/>
    you can switch back to default if needed.  <br/>
7 - configure your cron on the rpi as described above. <br/>
8 - enjoy ! ;) <br/>
