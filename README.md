# WorkNet-FileProvider

## How to deploy

```bash
sudo docker-compose up -d
```

## How to use

### upload

```bash
curl http://localhost:15000/file -X POST -F file=@/home/a/wall.png -F payload='{"filename":"wall23.obj","namespace":"user/shr","tags":["a"],"meta":{"faces":100}}'
```

### patch (partial update)


```bash
curl http://localhost:15000/file/user/shr:1 -X PATCH -d '{"tags":["b"],"meta":{"faces":1000}}'
```

### put (replace)
```bash
curl http://localhost:15000/file/user/shr:1 -X PUT -d '{"tags":["b"],"meta":{"faces":1000}}'
```

### query

for example:

```bash
curl http://localhost:15000/query
```

support Progrest's API query API http://postgrest.org/en/v5.2/api.html


### sql query

for example:

```bash
curl http://localhost:15000/sql/size>100 and extname='.obj' and namespace='common'
```

### get the file

for example:

```bash
curl http://localhost:15000/file/6,01da5e214b
```

`6,01da5e214b` is the `seaweedfs`'s file_id