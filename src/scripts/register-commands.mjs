import axios from "axios";
import chalk from "chalk";
import dotenv from "dotenv";
import fs from "fs-extra";
import { globby } from "globby";
import path from "path";

(async () => {
  dotenv.config();

  if (!process.env.APP_ID || !process.env.BOT_TOKEN) {
    throw "You must define APP_ID and BOT_TOKEN in .env file or in a command line run (e.g. APP_ID=1234 BOT_TOKEN=ABCD node register_commands/register.js)"
  }

  const commands = await loadCommands();
  const appId = process.env.APP_ID;
  const botToken = process.env.BOT_TOKEN;
  const headers = {
    "Authorization": `Bot ${botToken}`,
    "Content-Type": "application/json"
  };
  const globalUrl = `https://discord.com/api/v8/applications/${appId}/commands`;

  axios
    .put(globalUrl, JSON.stringify(commands), { headers })
    .then(() => console.log(`${chalk.cyan(commands.length)} global commands registered.`))
    .catch(e => {
      console.error(JSON.stringify(e.response.data, null, 2));
    });
})();

async function loadCommands() {
  const commandsDir = path.resolve("./src/Proxy/Commands");
  const commands = await globby("**/command*.json", { commandsDir });

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