FROM ubuntu:16.04
RUN useradd -ms /bin/bash unity
WORKDIR /home/unity
COPY builds/Server_Linux/linuxserver.x86_64 /home/unity/
COPY builds/Server_Linux/linuxserver_Data /home/unity/linuxserver_Data/
RUN chown -R unity:unity /home/unity/linuxserver*
USER unity

EXPOSE 7777-7787
ENV SERVERS_REGISTRY_URL http://jpgjsr.azurewebsites.net/api/servers
ENV HEARTBEAT_PERIOD 3

CMD ["./linuxserver.x86_64", "-logFile", "/dev/stdout", "-batchmode", "-nographics"]
