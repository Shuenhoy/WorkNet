from quart import Quart, websocket, request
import aiohttp
import asyncio
import asyncpg
import json
import os

from aiohttp import FormData

app = Quart(__name__)

@app.before_serving
async def create_aiohttp():
    app.session = aiohttp.ClientSession()
    app.db = await asyncpg.connect('postgres://postgre:password@database:5432/db')
    app.read_only_db = await asyncpg.connect('postgres://read_only_user:123456@database:5432/db')

async def post(url: str, data=None,params=None,headers=None):
    async with app.session.post(url,data=data,params=params,headers=headers) as response:
        html = await response.read()
        return {
            "status": response.status,
            "headers": response.headers,
            "resp": response,
            "html": html,
            "url": url,
        }

async def fetchs(url: str, params:dict=None):
    async with app.session.get(url,params=params) as response:
        html = await response.read()
        return {
            "status": response.status,
            "headers": response.headers,
            "resp": response,
            "html": html,
            "url": url,
        }
@app.route('/file/<path:namespace>:<id>', methods=['PATCH'])
async def patch_file(namespace,id):
    input = await request.get_json()
    exist = await app.read_only_db.fetchrow('SELECT * from api.files where id=$1 and namespace=$2',int(id),namespace)
    if exist==None:
        return '"{}:{}" not exists'.format(namespace,id), 400
    origin = dict(exist)
    new_tags = input.get('tags') or []
    new_meta = input.get('meta') or {}
    old_tags = origin.get('tags') or []
    old_meta = origin.get('meta') or {}

    await app.db.execute('UPDATE api.files set metadata=$1, tags=$2',
        json.dumps({**old_meta, **new_meta}),list(set(old_tags).union(set(new_tags))))
    return "ok"


@app.route('/file/<path:namespace>:<id>', methods=['PUT'])
async def put_file(namespace,id):
    input = await request.get_json()
    exist = await app.read_only_db.fetchrow('SELECT id from api.files where id=$1 and namespace=$2',int(id),namespace)
    if exist==None:
        return '"{}:{}" not exists'.format(namespace,id), 400
    origin = dict(exist)
    new_tags = input.get('tags') or []
    new_meta = input.get('meta') or {}

    await app.db.execute('UPDATE api.files set metadata=$1, tags=$2',
        json.dumps(new_meta),new_tags)
    return "ok"

# @app.route('/file/<path:namespace>:<id>', methods=['DELETE'])
# async def delete_file(namespace,id):
#     exist = await app.read_only_db.fetchrow('SELECT id from api.files where id=$1 and namespace=$2',int(id),namespace)
#     if exist==None:
#         return '"{}:{}" not exists'.format(namespace,id), 400
#     await app.db.execute('DELETA api.files  where id=$1 and namespace=$2',int(id),namespace)
#     return "ok"

@app.route('/file', methods=["POST"])
async def upload():
    resp = (await fetchs('http://master:9333/dir/assign'))
    file_id = (await resp["resp"].json())['fid']
    try:
        info = json.loads((await request.form)['payload'])
    except:
        return 'payload in json format should be provided!',400
    try:
        filename = info['filename']
    except:
        return '"filename" in payload should be provided!',400
    try:
        namespace = info['namespace']
    except:
        return '"namespace" in payload should be provided!',400

    exist = await app.read_only_db.fetchrow('SELECT * from api.files where filename=$1 and namespace=$2',filename,namespace)
    if exist:
        return '"{}:{}" exists'.format(namespace,filename), 400
    try:
        file = (await request.files)['file']
    except:
        return "file should be provided!",400
    data = FormData()
    data.add_field('file', file.stream)
    

    resp2 = await post('http://volume:18080/{}'.format(file_id),data=data)
    info2 = await resp2["resp"].json()

    await app.db.execute('''
        INSERT INTO api.files values(DEFAULT,$1,$2,$3,$4,$5,$6,$7,$8)
        ''',
        file_id,
        json.dumps(info.get('meta') or {}),
        info2['size'],
        info2['eTag'],
        info.get('tags'),
        os.path.splitext(info['filename'])[1],
        info['filename'],
        info['namespace'])
    return "ok"

@app.route('/file/<id>')
async def getfile(id: str):
    resp = await fetchs('http://volume:18080/{}'.format(id))
    return resp["html"], resp["status"], resp["headers"]

@app.route('/query')
async def query():
    resp = await fetchs('http://postgrest:3000/files',request.args)
    return resp["html"]

@app.route('/sql/<path:query>')
async def sql(query:str):
    print(query)
    rets = [dict(x) for x in await app.read_only_db.fetch('select * from api.files where {}'.format(query))]
    print(rets)
    return json.dumps(rets)

if __name__ == "__main__":
    app.run(host="0.0.0.0")
