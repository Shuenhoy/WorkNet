# WorkNet

## Deploy Message Queue with docker
`docker-compose -f docker/docker-compose.Rabbitmq.yml up -d`


## Deploy agents with docker
1. create `/home/worknode`
2. copy `./Agent/appsettings.json` to `/home/worknode` and modify it
3. `docker-compose -f docker/docker-compose.Agent.yml up -d`

## Client

copy `Client/worknet.json` to your home path, and modify the config. Then:

1. copy `wn_executor.lua` to a folder or write your own one
2. wn run ./your program
3. wn pull id

## Publish Excutable For Agent and Client

Execute the following commands in their project folders:

```bash
dotnet publish -r linux-x64 -c Release
```
