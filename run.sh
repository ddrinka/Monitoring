#!/bin/bash

docker run -d --restart always --network monitoring -v /var/local/influx:/var/lib/influxdb --name influxdb influxdb
docker run -d --restart always --network monitoring -v /etc/cm_passwd:/app/password.txt --name motorola_cm motorola_cm
docker run -d --restart always --network monitoring --name telegraf telegraf
