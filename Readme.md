# WorkNet

## Deploy with Docker
### FileProvider

```bash
docker-compose  -f docker/docker-compose.FileProvider.env.yml -f docker/docker-compose.FileProvider.yml up -d
```


### Server

* set up the environment

```bash
docker-compose  -f docker/docker-compose.Server.env.yml up -d
```

* login http://localhost:15672 with guest:guest
* add user

``` bash
docker-compose  -f docker/docker-compose.Server.yml up -d
```

### Run Server and Fileprovider in the same machine

* set up the environment

```bash
docker-compose  -f docker/docker-compose.ServerFileProvider.env.yml up -d
```

* login http://localhost:15672 with guest:guest
* add user `server:server`

``` bash
docker-compose  -f docker/docker-compose.Server.yml up -d
docker-compose  -f docker/docker-compose.FileProvider.yml up -d

```

## Develop with Docker

Setup the environment, then

```bash
docker-compose run -p xx:5000 docker/docker-compose.dev.yml web bash
```

## Publish Excutable For Agent and Client

Execute the following commands in their project folders:

```bash
dotnet publish -r linux-x64 -c Release
```
