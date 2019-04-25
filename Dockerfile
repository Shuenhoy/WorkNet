FROM python:3.7-alpine

RUN pip config set global.index-url https://mirrors.ustc.edu.cn/pypi/web/simple
RUN sed -i 's/dl-cdn.alpinelinux.org/mirrors.ustc.edu.cn/g' /etc/apk/repositories

COPY ./Pipfile /app/Pipfile
COPY ./Pipfile.lock /app/Pipfile.lock
WORKDIR /app
RUN pip install --no-cache-dir pipenv
RUN apk add --no-cache build-base \
    && pip install --no-cache-dir asyncpg\
    && apk del build-base 

RUN pipenv install --system
COPY . /app
CMD [ "hypercorn", "src/main:app", "-b", "0.0.0.0:5000" ]