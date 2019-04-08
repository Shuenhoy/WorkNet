FROM python:3.7-alpine

RUN pip config set global.index-url https://mirrors.ustc.edu.cn/pypi/web/simple
RUN sed -i 's/dl-cdn.alpinelinux.org/mirrors.ustc.edu.cn/g' /etc/apk/repositories

RUN pip install --no-cache-dir pipenv
RUN pip install --no-cache-dir coala coala-bears

RUN apk add --no-cache --virtual .build-deps \
    gcc \
    musl-dev \
    linux-headers \
    && pip install --no-cache-dir typed-ast mypy --pre \
    && apk del --no-cache .build-deps

COPY ./Pipfile /app/Pipfile
COPY ./Pipfile.lock /app/Pipfile.lock
WORKDIR /app
RUN pipenv install --system
COPY . /app



CMD  ["coala", "--non-interactive"]