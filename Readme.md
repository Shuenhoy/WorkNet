# WorkNet

## Deploy with Docker
### FileProvider

```bash
docker-compose up -f docker/docker-compose.FileProvider.env.yml -f docker/docker-compose.FileProvider.env.yml
```


### Server

* set up the environment

```bash
docker-compose up -f docker/docker-compose.Server.env.yml -d
```

* login http://localhost:15672 with guest:guest
* add user

``` bash
docker-compose up -f docker/docker-compose.Server.yml -d
```


## Develop with Docker

Setup the environment, then

```bash
docker-compose run -p xx:5000 docker/docker-compose.dev.yml web bash
```