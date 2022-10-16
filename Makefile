ifndef VERBOSE
MAKEFLAGS += --no-print-directory
endif

include .env

SHELL := /usr/bin/env  bash
.DEFAULT_GOAL := validate
git_branch=$(shell git symbolic-ref HEAD | sed -e 's,.*/\(.*\),\1,' | tr _ - | tr '[:upper:]' '[:lower:]')

stack_name = $(APP_NAME)-$(APP_ENVIRONMENT)-stack

$(info Main Stack:        $(stack_name))

all: validate build deploy artifacts
q: build deploy

validate:
	sam validate \
		--profile $(AWS_PROFILE) \
		--region $(AWS_REGION)

build:
	sam build \
		--cached \
		--profile $(AWS_PROFILE)

deploy:
	sam deploy \
		--profile $(AWS_PROFILE) \
		--region $(AWS_REGION) \
		--stack-name $(stack_name) \
		--resolve-s3 \
		--capabilities CAPABILITY_IAM \
		--no-fail-on-empty-changeset \
		--parameter-overrides AppIdentifier=$(APP_NAME) Environment=$(APP_ENVIRONMENT) BotPublicKey=$(PUBLIC_KEY) EspAuthToken=$(ESP_AUTH_TOKEN)

update_commands:
	node ./src/scripts/register-commands.mjs

artifacts:
	node ./src/scripts/uploadAssets.mjs $(APP_NAME)-$(APP_ENVIRONMENT)

delete:
	sam delete \
		--profile $(AWS_PROFILE) \
		--region $(AWS_REGION) \
		--stack-name $(stack_name)