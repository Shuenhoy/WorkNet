FROM python:3.8-alpine

RUN pip config set global.index-url https://mirrors.ustc.edu.cn/pypi/web/simple
RUN sed -i 's/dl-cdn.alpinelinux.org/mirrors.ustc.edu.cn/g' /etc/apk/repositories

RUN apk add --no-cache --virtual .build-deps \
    gcc \
    python3-dev \
    musl-dev \
    postgresql-dev \
    linux-headers \
    && pip install --no-cache-dir psycopg2 uwsgi \
    && apk del --no-cache .build-deps

RUN apk add --no-cache libpq

RUN pip install pipenv

COPY . /app

WORKDIR /app
RUN pipenv install --system
CMD pipenv run src/main.py