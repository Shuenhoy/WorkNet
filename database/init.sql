create schema api;

create table api.files (
  id serial primary key,
  seaweedid text not null unique,
  metadata jsonb not null default '{}',
  size int not null,
  eTag text not null,
  tags text[] default '{}',
  extname text not null,
  filename text not null,
  namespace text not null
);


create role read_only_user login noinherit  password '123456';
grant usage on schema api to read_only_user;
grant select on api.files to read_only_user;

create role web_anon nologin;

grant usage on schema api to web_anon;
grant all on api.files to web_anon;
grant usage, select on sequence api.files_id_seq to web_anon;

grant web_anon to postgre;
grant read_only_user to postgre;