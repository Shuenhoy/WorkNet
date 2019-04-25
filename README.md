# WorkNet-FileProvider

## How to deploy

```bash
sudo docker-compose up -d
```

## How to use

We recommand to use `jq` to prettier the JSON output.

### upload

```bash
curl -s 'http://localhost:15000/file' -X POST -F file=@/home/a/wall.png -F payload='{"filename":"wall23.obj","namespace":"user/shr","tags":["a"],"meta":{"faces":100}}' | jq
```

### patch (partial update)


```bash
curl -s 'http://localhost:15000/file/user/shr::1' -X PATCH -d '{"tags":["b"],"meta":{"faces":1000}}' | jq
# or
curl -s 'http://localhost:15000/file/user/shr:filename.obj' -X PATCH -d '{"tags":["b"],"meta":{"faces":1000}}' | jq
```

### put (replace)
```bash
curl -s 'http://localhost:15000/file/user/shr::1' -X PUT -d '{"tags":["b"],"meta":{"faces":1000}}' | jq
# or 
curl -s 'http://localhost:15000/file/user/shr:filename.obj' -X PUT -d '{"tags":["b"],"meta":{"faces":1000}}' | jq
```

### query

for example:

```bash
curl -s "http://localhost:15000/query?extname=eq..obj" | jq
```

support Progrest's API query API http://postgrest.org/en/v5.2/api.html


### sql query

for example:

```bash
curl -s "http://localhost:15000/sql/size>100 and extname='.obj' and namespace='common'" | jq
```

### get the file

for example:

```bash
curl "http://localhost:15000/file/6,01da5e214b"
# or 
curl "http://localhost:15000/file/user/shr:a.png"

```

`6,01da5e214b` is the `seaweedfs`'s file_id