# WorkNet

## Deploy

We have removed the server and fileprovider. We only need to depoly a rabbitmq broker.


## Deploy agents

Just run the program with proper config about the rabbitmq broker's information

## Client

1. write a `wn_executor.lua`
2. wn run ./your program
3. wn pull id

## Publish Excutable For Agent and Client

Execute the following commands in their project folders:

```bash
dotnet publish -r linux-x64 -c Release
```
