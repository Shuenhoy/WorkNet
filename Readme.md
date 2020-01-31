# WorkNet

## Deploy

We have removed the server and fileprovider. We only need to depoly a rabbitmq broker. You can use `docker/docker-compose.Rabbitmq.yml`


## Deploy agents

copy `Agent/appsettings.json` to a folder, and modify the config, then run `wn-agent` in that folder.

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
