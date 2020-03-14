## Design

### Submit Task

* `wn init [-f <json file name>] [-i <image>]  <command>` 
* `wn add <arguments>`
* `wn submit`
* `wn run [-i <image>] <command>`

### Query Results

* `wn watch`
* `wn query <json file name>`


## Example

```bash
wn init ./aplusb {a} {b}
wn add 3 5
wn add %[1..3] %[4,6..=10]
wn add ?[1 2 4 6] ?[9 1 2 2]
wn submit
```

```bash
wn init -f config.json cat {@file}

wn add data_a/a.txt
wn add ?[ data_b/*.txt ]
wn submit
```

```bash
wn run ./output ?[1.1,1.2,99.0] ?[100.0,98.0,0.0] ?[ data/*.obj ]
```

