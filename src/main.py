from quart import Quart, websocket, request
from quart.routing import BaseConverter
import aiohttp
import asyncio
import asyncpg
import json
import os

from aiohttp import FormData

app = Quart(__name__)

class AllConverter(BaseConverter):
    regex = r'.*'
    weight = 200

app.url_map.converters['all'] = AllConverter

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
async def fetch_stream(url: str, params:dict=None):
    async with app.session.get(url,params=params) as response:
        return response
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
@app.route('/<path:namespace>:<path:filename>', methods=['PATCH'])
async def patch_file_name(namespace,filename):
    
    input = await request.get_json(force=True)
    exist = await app.read_only_db.fetchrow('SELECT * from api.files where filename=$1 and namespace=$2',filename,namespace)
    if exist==None:
        return '"`{}:{}` not exists"'.format(namespace,filename), 400
    origin = dict(exist)
    new_tags = input.get('tags') or []
    new_meta = input.get('meta') or {}
    old_tags = origin.get('tags') or []
    old_meta = origin.get('metadata') or {}

    await app.db.execute('UPDATE api.files set metadata=$1::jsonb, tags=$2 where filename=$3 and namespace=$4',
        json.dumps({**old_meta, **new_meta}),list(set(old_tags).union(set(new_tags))),filename,namespace)
    return '"ok"'

@app.route('/<path:namespace>:<path:filename>', methods=['GET'])
async def get_file_by_name(namespace,filename):
    exist = await app.read_only_db.fetchrow('SELECT * from api.files where filename=$1 and namespace=$2',filename,namespace)
    if exist==None:
        return '"`{}:{}` not exists"'.format(namespace,filename), 400
    origin = dict(exist)
    resp = await fetch_stream('http://volume:18080/{}'.format(origin['seaweedid']))
    return resp.content, resp.status, resp.headers

@app.route('/<path:namespace>::<int:id>', methods=['PATCH'])
async def patch_file_id(namespace,id):
    try:
        id = int(id)
    except:
        return '"should provide id of int type"',400
    input = await request.get_json(force=True)
    exist = await app.read_only_db.fetchrow('SELECT * from api.files where id=$1 and namespace=$2',id,namespace)
    if exist==None:
        return '"`{}::{}` not exists"'.format(namespace,id), 400
    origin = dict(exist)
    new_tags = input.get('tags') or []
    new_meta = input.get('meta') or {}
    old_tags = origin.get('tags') or []
    old_meta = origin.get('metadata') or {}

    await app.db.execute('UPDATE api.files set metadata=$1::jsonb, tags=$2 where id=$3 and namespace=$4',
        json.dumps({**old_meta, **new_meta}),list(set(old_tags).union(set(new_tags))),id,namespace)
    return '"ok"'

@app.route('/<path:namespace>:<path:filename>', methods=['PUT'])
async def put_file_filename(namespace,filename):

    input = await request.get_json(force=True)
    exist = await app.read_only_db.fetchrow('SELECT * from api.files where filename=$1 and namespace=$2',filename,namespace)
    if exist==None:
        return '"`{}:{}` not exists"'.format(namespace,id), 400
    origin = dict(exist)
    print(origin, dict(input))
    new_tags = input.get('tags') or  origin.get('tags')  or []
    new_meta = input.get('meta') or origin.get('metadata') or {}
    
    await app.db.execute('UPDATE api.files set metadata=$1::jsonb, tags=$2 where filename=$3 and namespace=$4',
        json.dumps(new_meta),new_tags,filename,namespace)
    return '"ok"'

@app.route('/<path:namespace>::<int:id>', methods=['PUT'])
async def put_file_id(namespace,id):
    try:
        id = int(id)
    except:
        return '"should provide id of int type"',400
    input = await request.get_json(force=True)
    exist = await app.read_only_db.fetchrow('SELECT * from api.files where id=$1 and namespace=$2',id,namespace)
    if exist==None:
        return '"`{}:{}` not exists"'.format(namespace,id), 400
    origin = dict(exist)
    
    new_tags = input.get('tags') or  origin.get('tags')  or []
    new_meta = input.get('meta') or origin.get('metadata') or {}

    await app.db.execute('UPDATE api.files set metadata=$1::jsonb, tags=$2 where id=$3 and namespace=$4',
        json.dumps(new_meta),new_tags,id,namespace)
    return '"ok"'
@app.route('/<path:namespace>:<path:filename>', methods=["POST"])
async def upload_to(namespace, filename):
    resp = (await fetchs('http://master:9333/dir/assign'))
    file_id = (await resp["resp"].json())['fid']
    try:
        info = json.loads((await request.form)['payload'])
    except:
        info = {}
   
    exist = await app.read_only_db.fetchrow('SELECT * from api.files where filename=$1 and namespace=$2',filename,namespace)
    if exist:
        return '"`{}:{}` exists"'.format(namespace,filename), 400
    try:
        file = (await request.files)['file']
    except:
        return '"file should be provided!"',400
    data = FormData()
    data.add_field('file', file.stream)
    

    resp2 = await post('http://volume:18080/{}'.format(file_id),data=data)
    info2 = await resp2["resp"].json()

    id = await app.db.fetchval('''
        INSERT INTO api.files values(DEFAULT,$1,$2,$3,$4,$5,$6,$7,$8) RETURNING id
        ''',
        file_id,
        json.dumps(info.get('meta') or {}),
        info2['size'],
        info2['eTag'],
        info.get('tags'),
        os.path.splitext(filename)[1],
        filename,
        namespace)
    return json.dumps({"id":id,"seaweedfs_id":file_id})


@app.route('/', methods=["POST"])
async def upload():
    resp = (await fetchs('http://master:9333/dir/assign'))
    file_id = (await resp["resp"].json())['fid']
    try:
        info = json.loads((await request.form)['payload'])
    except:
        return '"payload in json format should be provided!"',400
    try:
        filename = info['filename']
    except:
        return '"`filename` in payload should be provided!"',400
    try:
        namespace = info['namespace']
    except:
        return '"`namespace` in payload should be provided!"',400

    exist = await app.read_only_db.fetchrow('SELECT * from api.files where filename=$1 and namespace=$2',filename,namespace)
    if exist:
        return '"`{}:{}` exists"'.format(namespace,filename), 400
    try:
        file = (await request.files)['file']
    except:
        return '"file should be provided!"',400
    data = FormData()
    data.add_field('file', file.stream)
    

    resp2 = await post('http://volume:18080/{}'.format(file_id),data=data)
    info2 = await resp2["resp"].json()

    id = await app.db.fetchval('''
        INSERT INTO api.files values(DEFAULT,$1,$2,$3,$4,$5,$6,$7,$8) RETURNING id
        ''',
        file_id,
        json.dumps(info.get('meta') or {}),
        info2['size'],
        info2['eTag'],
        info.get('tags'),
        os.path.splitext(info['filename'])[1],
        info['filename'],
        info['namespace'])
    return json.dumps({"id":id,"seaweedfs_id":file_id})

@app.route('/@id/<id>')
async def getfile(id: str):
    resp = await fetch_stream('http://volume:18080/{}'.format(id))
    return resp.content, resp.status, resp.headers

@app.route('/')
async def query():
    resp = await fetchs('http://postgrest:3000/files',request.args)
    return resp["html"]

@app.route('/@where/<all:query>')
async def sql(query:str):
    test=request.full_path[8:]

    try:
        rets = [dict(x) for x in await app.read_only_db.fetch('select * from api.files where {}'.format(test))]
    except:
        return '"sql error!"',400
    return json.dumps(rets)

if __name__ == "__main__":
    app.run(host="0.0.0.0")
