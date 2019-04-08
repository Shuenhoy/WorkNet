FROM 10.76.1.125:25000/infra/coala

COPY ./Pipfile /app/Pipfile
COPY ./Pipfile.lock /app/Pipfile.lock
WORKDIR /app
RUN pipenv install --system
COPY . /app

RUN coala --non-interactive