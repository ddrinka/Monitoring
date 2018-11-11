#!/bin/bash
docker build -t telegraf Monitoring/telegraf
docker build -t influxdb Monitoring/influxdb
docker build -t grafana Monitoring/grafana
docker build -t motorola_cm -f src/Monitoring.MotorolaCableModem.App/Dockerfile .
