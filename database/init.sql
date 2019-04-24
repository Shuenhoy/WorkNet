create schema api;

create table api.files (
  id serial primary key,
  seaweedid text not null unique,
  metadata jsonb not null default '{}',
  tags text[] default '{}',
  filename text not null,
  namespace text not null
);


create role read_only nologin;
grant usage on schema api to read_only;
grant select on api.files to read_only;

create role web_anon nologin;

grant usage on schema api to web_anon;
grant all on api.files to web_anon;
grant usage, select on sequence api.files_id_seq to web_anon;

grant web_anon to postgre;