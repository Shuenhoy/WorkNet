# WorkNet-FileProvider

## How to deploy

```bash
sudo docker-compose up -d
```

## How to use

We recommand to use `jq` to prettier the JSON output.

### upload

```bash
curl -s 'http://localhost:15000/' -X POST -F file=@/home/a/wall.png -F payload='{"filename":"wall23.obj","namespace":"user/shr","tags":["a"],"meta":{"faces":100}}' | jq
```

### patch (partial update)


```bash
curl -s 'http://localhost:15000/user/shr::1' -X PATCH -d '{"tags":["b"],"meta":{"faces":1000}}' | jq
# or
curl -s 'http://localhost:15000/user/shr:filename.obj' -X PATCH -d '{"tags":["b"],"meta":{"faces":1000}}' | jq
```

### put (replace)
```bash
curl -s 'http://localhost:15000/user/shr::1' -X PUT -d '{"tags":["b"],"meta":{"faces":1000}}' | jq
# or 
curl -s 'http://localhost:15000/user/shr:filename.obj' -X PUT -d '{"tags":["b"],"meta":{"faces":1000}}' | jq
```

### query

for example:

```bash
curl -s "http://localhost:15000/?extname=eq..obj" | jq
```

support Progrest's API query API http://postgrest.org/en/v5.2/api.html


### sql query

for example:

```bash
curl -s "http://localhost:15000/@where/size>100 and extname='.obj' and namespace='common'" | jq
```

### get the file

for example:

```bash
curl "http://localhost:15000/@id/,01da5e214b"
# or 
curl "http://localhost:15000/user/shr:a.png"

```

`6,01da5e214b` is the `seaweedfs`'s file_id

## Foci - File prOvider ClI 

first modify `base` in `./foci` to the real url, then

```bash
foci u[pload] <namespace>:<filename> <path_to_file> <payload>
  example: foci u shr:a.png a.png '{"tags":["image"]}'


foci q[uery] <query str>
  example: foci q extname=eq..png


foci s[ql] <sql str>
  example: foci s "namespace='shr' and extname='.obj'"


foci m[odify] <namespace>:<filename> <payload>
foci r[eplace] <namespace>:<filename> <payload>


foci d[ownload] <namespace>:<filename>
```