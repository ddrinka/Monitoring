FROM telegraf:1.8

#Enable non-free repo
RUN sed -i "s/stretch main/stretch main non-free/" /etc/apt/sources.list

#Install MIBs
RUN DEBIAN_FRONTEND=noninteractive apt-get update && \
    DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
        snmp-mibs-downloader && \
    rm -rf /var/lib/apt/lists/*

#Enable MIBs
RUN sed -i 's/mibs :/# mibs :/g' /etc/snmp/snmp.conf

COPY telegraf.conf /etc/telegraf/telegraf.conf
COPY hosts /tmp/hosts

CMD cat /tmp/hosts >> /etc/hosts; telegraf
