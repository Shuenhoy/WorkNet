FROM python:3.7-alpine

RUN pip config set global.index-url https://mirrors.ustc.edu.cn/pypi/web/simple
RUN sed -i 's/dl-cdn.alpinelinux.org/mirrors.ustc.edu.cn/g' /etc/apk/repositories

RUN pip install pipenv

COPY ./Pipfile /app/Pipfile
COPY ./Pipfile.lock /app/Pipfile.lock
WORKDIR /app

RUN pipenv install --system

CMD [ "/bin/sh" ]