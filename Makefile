ifndef VERBOSE
MAKEFLAGS += --no-print-directory
endif

SHELL := /usr/bin/env  bash
.DEFAULT_GOAL := validate
# git_branch=$(shell git symbolic-ref HEAD | sed -e 's,.*/\(.*\),\1,' | tr _ - | tr '[:upper:]' '[:lower:]')

app = lsnk
environment = dev
stack_name = load-shedding-discord-notifier-$(environment)-stack
profile_name = seriestracker-dev
region = eu-west-1
espAuthToken = zfw203PQjGIIrLTdScBf

$(info Main Stack:        $(stack_name))

all: validate build deploy artifacts
q: build deploy

local: local_build local_api

validate:
	sam validate \
		--profile $(profile_name) \
		--region $(region)

build:
	sam build \
		--cached \
		--profile $(profile_name)

deploy:
	sam deploy \
		--profile $(profile_name) \
		--region $(region) \
		--stack-name $(stack_name) \
		--resolve-s3 \
		--capabilities CAPABILITY_IAM \
		--no-fail-on-empty-changeset \
		--parameter-overrides AppIdentifier=$(app) Environment=$(environment) EspAuthToken=$(espAuthToken)

update_commands:
	node register_commands/register.js

artifacts:
	node ./src/scripts/uploadAssets.mjs $(app)v2-$(environment)

delete:
	sam delete \
		--profile $(profile_name) \
		--region $(region) \
		--stack-name $(stack_name)