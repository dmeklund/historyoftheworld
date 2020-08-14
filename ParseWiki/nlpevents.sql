-- auto-generated definition
create table nlpevents
(
    id          int auto_increment
        primary key,
    sentence    json   not null,
    startyear   int    not null,
    startmonth  int    null,
    startday    int    null,
    starthour   int    null,
    startminute int    null,
    endyear     int    not null,
    endmonth    int    null,
    endday      int    null,
    endhour     int    null,
    endminute   int    null,
    lat         double null,
    lng         double null,
    pageid      int    null
);

