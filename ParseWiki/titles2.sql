-- auto-generated definition
create table titles2
(
    id    int          not null
        primary key,
    title varchar(255) null
);

create index titles2_title_index
    on titles2 (title);

