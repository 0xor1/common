FROM mariadb:11.8.2
ENV MYSQL_ROOT_PASSWORD=root
COPY sql/common.sql /docker-entrypoint-initdb.d/dnsk.sql