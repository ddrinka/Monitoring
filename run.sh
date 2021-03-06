#!/bin/bash

docker run -d --restart always --network monitoring -v /var/local/influx:/var/lib/influxdb -p 8086:8086 --name influxdb influxdb
docker run -d --restart always --network monitoring -v /etc/cm_passwd:/app/password.txt --name motorola_cm motorola_cm
docker run -d --restart always --network monitoring -v /etc/nest_passwd:/app/password.txt --name nest nest
docker run -d --restart always --network monitoring -v /etc/meater_key:/etc/meater_key --name telegraf telegraf
docker run -d --restart always --network monitoring -v /var/local/grafana:/var/lib/grafana -p 80:3000 --name grafana grafana
