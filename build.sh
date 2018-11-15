#!/bin/bash
docker build -t telegraf telegraf
docker build -t influxdb influxdb
docker build -t grafana grafana
docker build -t motorola_cm -f src/Monitoring.MotorolaCableModem.App/Dockerfile .
docker build -t nest -f src/Monitoring.Nest.App/Dockerfile .
