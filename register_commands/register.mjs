// require('dotenv').config()
// const { register } = require("./misc/send");

import dotenv from "dotenv";
dotenv.config();

if (!process.env.APP_ID || !process.env.BOT_TOKEN) {
  throw 'You must define APP_ID and BOT_TOKEN in .env file or in a command line run (e.g. APP_ID=1234 BOT_TOKEN=ABCD node register_commands/register.js)'
}

import fs from 'fs-extra';
import { globby } from 'globby';
import path from 'path';
import axios from 'axios';
import chalk from "chalk";

(async () => {
  const appId = process.env.APP_ID;
  const botToken = process.env.BOT_TOKEN;
  const guildId = null;
  const commands = await loadCommands()
  const headers = {
    "Authorization": `Bot ${botToken}`,
    "Content-Type": "application/json"
  }

  const globalUrl = `https://discord.com/api/v8/applications/${appId}/commands`
  const guildUrl = `https://discord.com/api/v8/applications/${appId}/guilds/${guildId}/commands`
  const endpoint = guildId ? guildUrl : globalUrl
  const cmdInfo = guildId ? "Guild" : "Global"

  // console.log(endpoint);
  // console.log(JSON.stringify(commands, null, 2));

  // axios.put(endpoint, JSON.stringify(commands), { headers: headers })
  //   .then(() => console.log(`${cmdInfo} Commands registered.`))
  //   .catch(e => console.error(e))
})();

async function loadCommands() {
  const commandsDir = path.resolve('./src/Proxy/Commands');
  const commands = await globby("**/command.json", { commandsDir });

  let commandsOut = []

  console.log(chalk.cyan("Loading commands..."));
  for (const config of commands) {
    const commandString = await fs.readFile(config, { encoding: "utf8" });
    const command = JSON.parse(commandString);

    console.log(` > ${chalk.yellow(command.name)}`);

    commandsOut.push(command);
  }

  return commandsOut
}