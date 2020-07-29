PHONY: build up down db

build:
		docker-compose build .

up:
		docker-compose up -d

down:
		docker-compose down

db:
		docker-compose up -d statisticsdb